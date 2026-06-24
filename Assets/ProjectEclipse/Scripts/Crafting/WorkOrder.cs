using System.Collections.Generic;
using ProjectEclipse.Equipment;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Crafting
{
    public class WorkOrder
    {
        private const string MaterialsChangedBlockReason = "Queued ingredients changed before processing could start.";

        private class ActiveCraftingJob
        {
            public CraftingPlanStep Step;
            public float TotalSeconds;
            public float RemainingSeconds;
        }

        private readonly List<ActiveCraftingJob> activeJobs = new List<ActiveCraftingJob>();
        private readonly Dictionary<ItemDefinition, int> consumedInputs = new Dictionary<ItemDefinition, int>();
        private readonly int initialFinalOwned;
        private int completedFinalQuantity;

        public CraftingPlan Plan { get; private set; }
        public bool IsComplete { get; private set; }
        public bool IsCanceled { get; private set; }
        public string CompletionCue { get; private set; }
        public AudioClip CompletionSound { get; private set; }

        public WorkOrder(CraftingPlan plan, int initialFinalOwned)
        {
            Plan = plan;
            this.initialFinalOwned = Mathf.Max(0, initialFinalOwned);
        }

        public void Cancel()
        {
            IsCanceled = true;
            activeJobs.Clear();
            if (Plan == null)
            {
                return;
            }

            for (int i = 0; i < Plan.Steps.Count; i++)
            {
                CraftingPlanStep step = Plan.Steps[i];
                if (step != null && step.State != CraftingPlanStepState.Complete)
                {
                    step.State = CraftingPlanStepState.Blocked;
                    step.Progress01 = 0f;
                    step.RemainingSeconds = 0f;
                    step.BlockReason = "Work Order canceled.";
                }
            }
        }

        public void Tick(InventoryStore inventory, InventoryCraftingController inventoryCrafting, EquipmentController equipment, out CraftingFeedbackMessage feedback)
        {
            feedback = null;
            if (IsComplete || IsCanceled || Plan == null || inventory == null)
            {
                return;
            }

            ReopenRetryableBlockedSteps(inventory, inventoryCrafting);
            UpdateActiveJobs(inventory, equipment, out feedback);
            if (feedback != null || IsComplete)
            {
                return;
            }

            TryStartPendingSteps(inventory, inventoryCrafting, out feedback);
        }

        public int CountReserved(ItemDefinition item)
        {
            if (Plan == null || item == null)
            {
                return 0;
            }

            return Mathf.Max(0, Plan.GetReserved(item) - GetConsumed(item));
        }

        public List<CraftingRequirementLine> GetRequirementLines(InventoryStore inventory)
        {
            return GetRequirementLines(inventory, null);
        }

        public List<CraftingRequirementLine> GetRequirementLines(InventoryStore inventory, InventoryCraftingController inventoryCrafting)
        {
            List<CraftingRequirementLine> lines = new List<CraftingRequirementLine>();
            if (Plan == null || Plan.FinalRecipe == null)
            {
                return lines;
            }

            List<CraftingPlanStep> stationLinesAdded = new List<CraftingPlanStep>();
            ItemDefinition finalItem = Plan.FinalRecipe.OutputItem;
            int finalOwned = inventory != null && finalItem != null ? inventory.CountItem(finalItem) : 0;
            int finalProgress = IsComplete ? Mathf.Clamp(completedFinalQuantity, 0, Plan.TargetQuantity) : Mathf.Clamp(finalOwned - initialFinalOwned, 0, Plan.TargetQuantity);
            CraftingPlanStep finalStep = GetFinalStep();
            CraftingRequirementStatus finalStatus = IsComplete
                ? CraftingRequirementStatus.Complete
                : finalStep != null && finalStep.State == CraftingPlanStepState.Processing
                    ? CraftingRequirementStatus.Processing
                    : CraftingRequirementStatus.None;
            string finalDetail = IsComplete
                ? GetCompletionDetail()
                : finalStep != null && finalStep.State == CraftingPlanStepState.Processing
                    ? GetProcessingDetail(finalStep)
                    : string.Empty;

            lines.Add(new CraftingRequirementLine(
                finalItem,
                finalItem != null ? finalItem.DisplayName : "Final Item",
                finalProgress,
                Plan.TargetQuantity,
                0,
                finalStatus,
                finalDetail));

            AppendStationLineForStep(lines, finalStep, inventoryCrafting, stationLinesAdded);

            HashSet<ItemDefinition> addedItems = new HashSet<ItemDefinition>();
            for (int i = 0; i < Plan.FinalRecipe.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = Plan.FinalRecipe.Ingredients[i];
                if (ingredient == null || ingredient.Item == null)
                {
                    continue;
                }

                AddRequirementLine(lines, ingredient.Item, null, inventory, inventoryCrafting, addedItems);
                CraftingPlanStep producerStep = GetOutputStep(ingredient.Item);
                AppendStationLineForStep(lines, producerStep, inventoryCrafting, stationLinesAdded);
                AppendProducerIngredientLines(lines, producerStep, inventory, inventoryCrafting, addedItems);
            }

            List<KeyValuePair<ItemDefinition, int>> remainingRequirements = new List<KeyValuePair<ItemDefinition, int>>();
            foreach (KeyValuePair<ItemDefinition, int> pair in Plan.Requirements)
            {
                if (!addedItems.Contains(pair.Key))
                {
                    remainingRequirements.Add(pair);
                }
            }

            remainingRequirements.Sort((left, right) => string.Compare(
                left.Key != null ? left.Key.DisplayName : string.Empty,
                right.Key != null ? right.Key.DisplayName : string.Empty));
            for (int i = 0; i < remainingRequirements.Count; i++)
            {
                AddRequirementLine(lines, remainingRequirements[i].Key, null, inventory, inventoryCrafting, addedItems);
            }

            AppendCurrentStationLines(lines, inventoryCrafting, stationLinesAdded);

            for (int i = 0; i < Plan.BlockingMessages.Count; i++)
            {
                if (inventoryCrafting != null && IsStationBlockingMessage(Plan.BlockingMessages[i]))
                {
                    continue;
                }

                lines.Add(new CraftingRequirementLine(
                    null,
                    Plan.BlockingMessages[i],
                    0,
                    1,
                    0,
                    BlockingStatus(Plan.BlockingMessages[i]),
                    Plan.BlockingMessages[i]));
            }

            return lines;
        }

        private void AddRequirementLine(
            List<CraftingRequirementLine> lines,
            ItemDefinition item,
            string labelOverride,
            InventoryStore inventory,
            InventoryCraftingController inventoryCrafting,
            HashSet<ItemDefinition> addedItems)
        {
            if (lines == null || item == null || addedItems == null || addedItems.Contains(item))
            {
                return;
            }

            int required;
            if (!Plan.Requirements.TryGetValue(item, out required))
            {
                return;
            }

            int actualOwned = inventory != null ? inventory.CountItem(item) : 0;
            int consumed = GetConsumed(item);
            int owned = actualOwned + consumed;
            int reserved = CountReserved(item);
            int reservedDisplay = reserved + consumed;
            string label = !string.IsNullOrEmpty(labelOverride) ? labelOverride : item.DisplayName;
            CraftingRequirementStatus status = GetRequirementStatus(item, owned, required, reservedDisplay, inventory, inventoryCrafting);
            if (consumed > 0 && owned >= required)
            {
                status = CraftingRequirementStatus.Satisfied;
            }

            lines.Add(new CraftingRequirementLine(
                item,
                label,
                owned,
                required,
                reservedDisplay,
                status,
                GetRequirementDetail(item, actualOwned, owned, required, reserved, consumed, inventory, inventoryCrafting)));
            addedItems.Add(item);
        }

        private void AppendProducerIngredientLines(
            List<CraftingRequirementLine> lines,
            CraftingPlanStep producerStep,
            InventoryStore inventory,
            InventoryCraftingController inventoryCrafting,
            HashSet<ItemDefinition> addedItems)
        {
            if (producerStep == null || producerStep.Recipe == null)
            {
                return;
            }

            for (int i = 0; i < producerStep.Recipe.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = producerStep.Recipe.Ingredients[i];
                if (ingredient == null || ingredient.Item == null)
                {
                    continue;
                }

                AddRequirementLine(
                    lines,
                    ingredient.Item,
                    "  " + ingredient.Item.DisplayName,
                    inventory,
                    inventoryCrafting,
                    addedItems);
            }
        }

        private void UpdateActiveJobs(InventoryStore inventory, EquipmentController equipment, out CraftingFeedbackMessage feedback)
        {
            feedback = null;
            for (int i = activeJobs.Count - 1; i >= 0; i--)
            {
                ActiveCraftingJob job = activeJobs[i];
                job.RemainingSeconds -= Time.deltaTime;
                if (job.Step != null)
                {
                    job.Step.RemainingSeconds = Mathf.Max(0f, job.RemainingSeconds);
                    job.Step.Progress01 = job.TotalSeconds <= 0f ? 1f : Mathf.Clamp01(1f - (job.RemainingSeconds / job.TotalSeconds));
                }

                if (job.RemainingSeconds > 0f)
                {
                    continue;
                }

                CompleteJob(job, inventory, equipment, out feedback);
                activeJobs.RemoveAt(i);
                if (feedback != null)
                {
                    return;
                }
            }
        }

        private void TryStartPendingSteps(InventoryStore inventory, InventoryCraftingController inventoryCrafting, out CraftingFeedbackMessage feedback)
        {
            feedback = null;
            for (int i = 0; i < Plan.Steps.Count; i++)
            {
                CraftingPlanStep step = Plan.Steps[i];
                if (step == null || step.State != CraftingPlanStepState.Pending)
                {
                    continue;
                }

                if (!CanUseStation(step, inventoryCrafting) || !HasIngredients(step, inventory))
                {
                    continue;
                }

                if (!HasFreeLane(step, inventoryCrafting))
                {
                    continue;
                }

                if (!ConsumeIngredients(step, inventory))
                {
                    step.State = CraftingPlanStepState.Blocked;
                    step.BlockReason = MaterialsChangedBlockReason;
                    feedback = BuildFeedback("Insufficient Materials", MaterialsChangedBlockReason, true, false, GetRequirementLines(inventory, inventoryCrafting));
                    return;
                }

                StartJob(step, inventoryCrafting);
            }
        }

        private void StartJob(CraftingPlanStep step, InventoryCraftingController inventoryCrafting)
        {
            float totalSeconds = GetCraftTime(step, inventoryCrafting);
            if (totalSeconds <= 0.001f)
            {
                totalSeconds = 0.001f;
            }

            step.State = CraftingPlanStepState.Processing;
            step.Progress01 = 0f;
            step.RemainingSeconds = totalSeconds;
            activeJobs.Add(new ActiveCraftingJob
            {
                Step = step,
                TotalSeconds = totalSeconds,
                RemainingSeconds = totalSeconds
            });
        }

        private void CompleteJob(ActiveCraftingJob job, InventoryStore inventory, EquipmentController equipment, out CraftingFeedbackMessage feedback)
        {
            feedback = null;
            if (job == null || job.Step == null || job.Step.Recipe == null)
            {
                return;
            }

            CraftingPlanStep step = job.Step;
            if (step.OutputItem != null)
            {
                inventory.AddItem(step.OutputItem, step.OutputQuantity);
                if (step.IsFinalStep && step.Recipe.EquipOutputIfWeapon && equipment != null)
                {
                    equipment.TryEquipFromStorage(step.OutputItem);
                }
            }

            step.State = CraftingPlanStepState.Complete;
            step.Progress01 = 1f;
            step.RemainingSeconds = 0f;

            if (step.IsFinalStep)
            {
                IsComplete = true;
                completedFinalQuantity = step.OutputQuantity;
                CompletionCue = step.Recipe.CompletionCueText;
                CompletionSound = step.Recipe.CompletionSound;
                string detail = step.OutputItem != null ? step.OutputItem.DisplayName + " crafted!" : "Work Order complete.";
                if (!string.IsNullOrEmpty(CompletionCue))
                {
                    detail += " " + CompletionCue;
                }

                feedback = BuildFeedback("Work Order Complete", detail, false, true, GetRequirementLines(inventory));
            }
        }

        private bool HasIngredients(CraftingPlanStep step, InventoryStore inventory)
        {
            if (step == null || step.Recipe == null || inventory == null)
            {
                return false;
            }

            for (int i = 0; i < step.Recipe.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = step.Recipe.Ingredients[i];
                if (ingredient == null || ingredient.Item == null)
                {
                    continue;
                }

                if (!inventory.HasItem(ingredient.Item, ingredient.Quantity * step.CraftCount))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ConsumeIngredients(CraftingPlanStep step, InventoryStore inventory)
        {
            List<CraftingIngredient> ingredients = new List<CraftingIngredient>();
            for (int i = 0; i < step.Recipe.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = step.Recipe.Ingredients[i];
                if (ingredient != null && ingredient.Item != null)
                {
                    ingredients.Add(new CraftingIngredient(ingredient.Item, ingredient.Quantity * step.CraftCount));
                }
            }

            if (!inventory.ConsumeIngredients(ingredients))
            {
                return false;
            }

            for (int i = 0; i < ingredients.Count; i++)
            {
                CraftingIngredient ingredient = ingredients[i];
                if (ingredient != null)
                {
                    RecordConsumed(ingredient.Item, ingredient.Quantity);
                }
            }

            return true;
        }

        private bool CanUseStation(CraftingPlanStep step, InventoryCraftingController inventoryCrafting)
        {
            if (step == null || step.Recipe == null)
            {
                return false;
            }

            if (step.StationType == CraftingStationType.Inventory)
            {
                return true;
            }

            CraftingPortDefinition port = inventoryCrafting != null ? inventoryCrafting.GetPort(step.StationType) : null;
            if (port == null || port.PortLevel < step.RequiredPortLevel)
            {
                return false;
            }

            if (port.AllowedRecipes.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < port.AllowedRecipes.Count; i++)
            {
                if (port.AllowedRecipes[i] == step.Recipe)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasFreeLane(CraftingPlanStep step, InventoryCraftingController inventoryCrafting)
        {
            int lanes = GetLaneCount(step, inventoryCrafting);
            int active = 0;
            for (int i = 0; i < activeJobs.Count; i++)
            {
                if (activeJobs[i] != null && activeJobs[i].Step != null && activeJobs[i].Step.StationType == step.StationType)
                {
                    active++;
                }
            }

            return active < lanes;
        }

        private int GetLaneCount(CraftingPlanStep step, InventoryCraftingController inventoryCrafting)
        {
            if (step == null || step.StationType == CraftingStationType.Inventory)
            {
                return 999;
            }

            CraftingPortDefinition port = inventoryCrafting != null ? inventoryCrafting.GetPort(step.StationType) : null;
            return port != null ? port.LaneCount : 0;
        }

        private float GetCraftTime(CraftingPlanStep step, InventoryCraftingController inventoryCrafting)
        {
            if (step == null)
            {
                return 0f;
            }

            float seconds = step.BaseCraftTimeSeconds;
            if (step.StationType == CraftingStationType.Inventory)
            {
                return Mathf.Max(0.1f, seconds);
            }

            CraftingPortDefinition port = inventoryCrafting != null ? inventoryCrafting.GetPort(step.StationType) : null;
            float speed = port != null ? port.SpeedMultiplier : 1f;
            return Mathf.Max(0.1f, seconds / Mathf.Max(0.01f, speed));
        }

        private CraftingRequirementStatus GetRequirementStatus(
            ItemDefinition item,
            int owned,
            int required,
            int reserved,
            InventoryStore inventory,
            InventoryCraftingController inventoryCrafting)
        {
            if (IsOutputBlocked(item))
            {
                return CraftingRequirementStatus.Missing;
            }

            CraftingPlanStep outputStep = GetOutputStep(item);
            if (outputStep != null && outputStep.State == CraftingPlanStepState.Processing)
            {
                return CraftingRequirementStatus.Processing;
            }

            if (outputStep != null && outputStep.State == CraftingPlanStepState.Pending)
            {
                string missingLabel;
                int missingQuantity;
                if (TryGetMissingIngredientForStep(outputStep, inventory, out missingLabel, out missingQuantity))
                {
                    return CraftingRequirementStatus.Missing;
                }

                return CraftingRequirementStatus.Queueable;
            }

            if (required <= 0 || owned >= required)
            {
                return reserved > 0 ? CraftingRequirementStatus.Reserved : CraftingRequirementStatus.Satisfied;
            }

            int missing;
            if (Plan.MissingMaterials.TryGetValue(item, out missing) && owned < required)
            {
                return CraftingRequirementStatus.Missing;
            }

            if (reserved > 0)
            {
                return CraftingRequirementStatus.Reserved;
            }

            return CraftingRequirementStatus.Missing;
        }

        private string GetRequirementDetail(
            ItemDefinition item,
            int actualOwned,
            int owned,
            int required,
            int reserved,
            int consumed,
            InventoryStore inventory,
            InventoryCraftingController inventoryCrafting)
        {
            CraftingPlanStep blockedStep = GetOutputStep(item, CraftingPlanStepState.Blocked);
            if (blockedStep != null && !string.IsNullOrEmpty(blockedStep.BlockReason))
            {
                return blockedStep.BlockReason;
            }

            CraftingPlanStep outputStep = GetOutputStep(item);
            if (outputStep != null && outputStep.State == CraftingPlanStepState.Processing)
            {
                return GetProcessingDetail(outputStep);
            }

            if (outputStep != null && outputStep.State == CraftingPlanStepState.Pending)
            {
                string missingLabel;
                int missingQuantity;
                if (TryGetMissingIngredientForStep(outputStep, inventory, out missingLabel, out missingQuantity))
                {
                    return "Missing " + missingLabel + " x" + missingQuantity;
                }

                if (!CanUseStation(outputStep, inventoryCrafting))
                {
                    return "Waiting for " + outputStep.StationType;
                }

                return HasIngredients(outputStep, inventory) ? "Ready to process" : "Queued";
            }

            if (consumed > 0 && owned >= required)
            {
                return "Consumed by Work Order";
            }

            if (owned >= required)
            {
                return reserved > 0 ? "Reserved" : "Ready";
            }

            return "Missing " + Mathf.Max(0, required - owned) + " (available " + actualOwned + ")";
        }

        private bool IsOutputProcessing(ItemDefinition item)
        {
            return GetOutputStep(item, CraftingPlanStepState.Processing) != null;
        }

        private bool IsOutputPending(ItemDefinition item)
        {
            return GetOutputStep(item, CraftingPlanStepState.Pending) != null;
        }

        private bool IsOutputBlocked(ItemDefinition item)
        {
            return GetOutputStep(item, CraftingPlanStepState.Blocked) != null;
        }

        private CraftingPlanStep GetOutputStep(ItemDefinition item)
        {
            if (item == null || Plan == null)
            {
                return null;
            }

            for (int i = 0; i < Plan.Steps.Count; i++)
            {
                CraftingPlanStep step = Plan.Steps[i];
                if (step != null && step.OutputItem == item)
                {
                    return step;
                }
            }

            return null;
        }

        private CraftingPlanStep GetOutputStep(ItemDefinition item, CraftingPlanStepState state)
        {
            if (item == null || Plan == null)
            {
                return null;
            }

            for (int i = 0; i < Plan.Steps.Count; i++)
            {
                CraftingPlanStep step = Plan.Steps[i];
                if (step != null && step.OutputItem == item && step.State == state)
                {
                    return step;
                }
            }

            return null;
        }

        private CraftingPlanStep GetFinalStep()
        {
            if (Plan == null)
            {
                return null;
            }

            for (int i = 0; i < Plan.Steps.Count; i++)
            {
                CraftingPlanStep step = Plan.Steps[i];
                if (step != null && step.IsFinalStep)
                {
                    return step;
                }
            }

            return null;
        }

        private bool TryGetMissingIngredientForStep(CraftingPlanStep step, InventoryStore inventory, out string missingLabel, out int missingQuantity)
        {
            missingLabel = string.Empty;
            missingQuantity = 0;
            if (step == null || step.Recipe == null)
            {
                return false;
            }

            for (int i = 0; i < step.Recipe.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = step.Recipe.Ingredients[i];
                if (ingredient == null || ingredient.Item == null)
                {
                    continue;
                }

                CraftingPlanStep producer = GetOutputStep(ingredient.Item);
                if (producer != null && producer.State != CraftingPlanStepState.Complete)
                {
                    continue;
                }

                int requiredQuantity = ingredient.Quantity * step.CraftCount;
                int effectiveOwned = (inventory != null ? inventory.CountItem(ingredient.Item) : 0) + GetConsumed(ingredient.Item);
                if (effectiveOwned < requiredQuantity)
                {
                    missingLabel = ingredient.Item.DisplayName;
                    missingQuantity = requiredQuantity - effectiveOwned;
                    return true;
                }
            }

            return false;
        }

        private void ReopenRetryableBlockedSteps(InventoryStore inventory, InventoryCraftingController inventoryCrafting)
        {
            if (Plan == null)
            {
                return;
            }

            for (int i = 0; i < Plan.Steps.Count; i++)
            {
                CraftingPlanStep step = Plan.Steps[i];
                if (step == null || step.State != CraftingPlanStepState.Blocked || step.BlockReason != MaterialsChangedBlockReason)
                {
                    continue;
                }

                if (CanUseStation(step, inventoryCrafting) && HasIngredients(step, inventory))
                {
                    step.State = CraftingPlanStepState.Pending;
                    step.BlockReason = string.Empty;
                    step.Progress01 = 0f;
                    step.RemainingSeconds = 0f;
                }
            }
        }

        private void RecordConsumed(ItemDefinition item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                return;
            }

            int existing;
            consumedInputs.TryGetValue(item, out existing);
            consumedInputs[item] = existing + quantity;
        }

        private int GetConsumed(ItemDefinition item)
        {
            int value;
            return item != null && consumedInputs.TryGetValue(item, out value) ? value : 0;
        }

        private string GetCompletionDetail()
        {
            if (string.IsNullOrEmpty(CompletionCue))
            {
                return "Complete";
            }

            return "Complete - " + CompletionCue;
        }

        private string GetProcessingDetail(CraftingPlanStep step)
        {
            if (step == null)
            {
                return string.Empty;
            }

            int progress = Mathf.RoundToInt(step.Progress01 * 100f);
            return "Processing " + progress + "%, " + step.RemainingSeconds.ToString("0.0") + "s";
        }

        private void AppendCurrentStationLines(
            List<CraftingRequirementLine> lines,
            InventoryCraftingController inventoryCrafting,
            List<CraftingPlanStep> stationLinesAdded)
        {
            if (lines == null || inventoryCrafting == null || Plan == null)
            {
                return;
            }

            for (int i = 0; i < Plan.Steps.Count; i++)
            {
                CraftingPlanStep step = Plan.Steps[i];
                if (step == null || step.Recipe == null || step.State == CraftingPlanStepState.Complete || step.StationType == CraftingStationType.Inventory)
                {
                    continue;
                }

                AppendStationLineForStep(lines, step, inventoryCrafting, stationLinesAdded);
            }
        }

        private void AppendStationLineForStep(
            List<CraftingRequirementLine> lines,
            CraftingPlanStep step,
            InventoryCraftingController inventoryCrafting,
            List<CraftingPlanStep> stationLinesAdded)
        {
            if (lines == null || step == null || step.Recipe == null || step.StationType == CraftingStationType.Inventory)
            {
                return;
            }

            if (stationLinesAdded != null && stationLinesAdded.Contains(step))
            {
                return;
            }

            CraftingPortDefinition port = inventoryCrafting != null ? inventoryCrafting.GetPort(step.StationType) : null;
            if (port == null)
            {
                string stationName = CraftingTerminology.GetStationDisplayName(step.StationType);
                lines.Add(new CraftingRequirementLine(
                    null,
                    "Missing " + stationName,
                    0,
                    1,
                    0,
                    CraftingRequirementStatus.MissingPort,
                    "Equip " + stationName + " for " + step.Recipe.DisplayName));
                if (stationLinesAdded != null)
                {
                    stationLinesAdded.Add(step);
                }
                return;
            }

            if (port.PortLevel < step.RequiredPortLevel)
            {
                lines.Add(new CraftingRequirementLine(
                    port,
                    port.DisplayName,
                    port.PortLevel,
                    step.RequiredPortLevel,
                    0,
                    CraftingRequirementStatus.InsufficientPortTier,
                    "Upgrade required for " + step.Recipe.DisplayName));
                if (stationLinesAdded != null)
                {
                    stationLinesAdded.Add(step);
                }
                return;
            }

            if (port.AllowedRecipes.Count > 0 && !PortAllowsRecipe(port, step.Recipe))
            {
                lines.Add(new CraftingRequirementLine(
                    port,
                    step.Recipe.DisplayName,
                    0,
                    1,
                    0,
                    CraftingRequirementStatus.RecipeLocked,
                    "Recipe locked on " + port.DisplayName));
                if (stationLinesAdded != null)
                {
                    stationLinesAdded.Add(step);
                }
            }
        }

        private static bool PortAllowsRecipe(CraftingPortDefinition port, CraftingRecipe recipe)
        {
            if (port == null || recipe == null)
            {
                return false;
            }

            for (int i = 0; i < port.AllowedRecipes.Count; i++)
            {
                if (port.AllowedRecipes[i] == recipe)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsStationBlockingMessage(string message)
        {
            return message != null
                && (message.Contains("Missing Crafting Trinket")
                    || message.Contains("Insufficient Crafting Trinket Tier")
                    || message.Contains(" is not allowed by "));
        }

        private static CraftingRequirementStatus BlockingStatus(string message)
        {
            if (message != null && message.Contains("Missing Crafting Trinket"))
            {
                return CraftingRequirementStatus.MissingPort;
            }

            if (message != null && message.Contains("Insufficient Crafting Trinket Tier"))
            {
                return CraftingRequirementStatus.InsufficientPortTier;
            }

            if (message != null && message.Contains("Recipe Locked"))
            {
                return CraftingRequirementStatus.RecipeLocked;
            }

            return CraftingRequirementStatus.Missing;
        }

        private static CraftingFeedbackMessage BuildFeedback(
            string header,
            string detail,
            bool isError,
            bool isSuccess,
            IEnumerable<CraftingRequirementLine> lines)
        {
            return new CraftingFeedbackMessage(header, detail, isError, isSuccess, lines);
        }
    }
}
