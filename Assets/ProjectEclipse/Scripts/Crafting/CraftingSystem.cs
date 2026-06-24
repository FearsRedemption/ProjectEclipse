using System.Collections.Generic;
using ProjectEclipse.Equipment;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Crafting
{
    public class CraftingSystem : MonoBehaviour
    {
        [SerializeField] private List<CraftingRecipe> recipes = new List<CraftingRecipe>();

        private InventoryStore inventory;
        private EquipmentController equipment;
        private InventoryCraftingController inventoryCrafting;
        private CraftingPlanner planner;
        private WorkOrder activeWorkOrder;
        private CraftingFeedbackMessage feedback;
        private bool validatedRecipeData;

        public IReadOnlyList<CraftingRecipe> Recipes { get { return recipes; } }
        public WorkOrder ActiveWorkOrder { get { return activeWorkOrder; } }
        public CraftingFeedbackMessage Feedback { get { return feedback; } }

        public void Initialize(InventoryStore store, EquipmentController playerEquipment, IEnumerable<CraftingRecipe> availableRecipes)
        {
            inventory = store;
            equipment = playerEquipment;
            recipes = new List<CraftingRecipe>(availableRecipes);
            inventoryCrafting = store != null ? store.GetComponent<InventoryCraftingController>() : null;
            if (inventoryCrafting == null && store != null)
            {
                inventoryCrafting = store.gameObject.AddComponent<InventoryCraftingController>();
            }
            if (inventoryCrafting != null)
            {
                inventoryCrafting.Initialize(store);
            }
            planner = new CraftingPlanner(recipes, inventory, inventoryCrafting);
            ValidateRecipeDataOnce();
        }

        private void Update()
        {
            if (activeWorkOrder == null)
            {
                return;
            }

            CraftingFeedbackMessage workOrderFeedback;
            activeWorkOrder.Tick(inventory, inventoryCrafting, equipment, out workOrderFeedback);
            if (workOrderFeedback != null)
            {
                feedback = workOrderFeedback;
                PlayCompletionCueIfNeeded();
            }
        }

        public bool CanCraft(CraftingRecipe recipe)
        {
            if (recipe == null || inventory == null || !inventory.HasIngredients(recipe.Ingredients))
            {
                return false;
            }

            return HasRequiredStation(recipe);
        }

        public bool TryCraft(CraftingRecipe recipe)
        {
            return TryStartWorkOrder(recipe, recipe != null ? recipe.OutputQuantity : 1);
        }

        public bool TryCraftImmediate(CraftingRecipe recipe)
        {
            if (!CanCraft(recipe) || !inventory.ConsumeIngredients(recipe.Ingredients))
            {
                return false;
            }

            inventory.AddItem(recipe.OutputItem, recipe.OutputQuantity);

            if (recipe.EquipOutputIfWeapon && equipment != null)
            {
                WeaponDefinition weapon = recipe.OutputItem as WeaponDefinition;
                if (weapon != null)
                {
                    equipment.TryEquipWeapon(weapon);
                }
            }

            return true;
        }

        public bool TryStartWorkOrder(CraftingRecipe recipe, int targetQuantity)
        {
            if (recipe == null || recipe.OutputItem == null || inventory == null)
            {
                feedback = new CraftingFeedbackMessage("Recipe Locked", "Recipe data is incomplete.", true, false, null);
                return false;
            }

            if (activeWorkOrder != null && !activeWorkOrder.IsComplete && !activeWorkOrder.IsCanceled)
            {
                feedback = new CraftingFeedbackMessage("Work Order Already Active", "Cancel the current Work Order before starting another.", true, false, activeWorkOrder.GetRequirementLines(inventory, inventoryCrafting));
                return false;
            }

            EnsurePlanner();
            CraftingPlan plan = planner.BuildPlan(recipe, Mathf.Clamp(targetQuantity, 1, 9999));
            if (plan.HasLoopError)
            {
                feedback = new CraftingFeedbackMessage("Recipe Locked", "Dependency loop detected. Work Order was not started.", true, false, BuildPlanLines(plan));
                return false;
            }

            if (HasStationBlockingProblem(plan))
            {
                WorkOrder previewOrder = new WorkOrder(plan, inventory.CountItem(recipe.OutputItem));
                feedback = new CraftingFeedbackMessage(
                    GetBlockingHeader(plan),
                    BuildStationBlockingDetail(plan),
                    true,
                    false,
                    previewOrder.GetRequirementLines(inventory, inventoryCrafting));
                return false;
            }

            activeWorkOrder = new WorkOrder(plan, inventory.CountItem(recipe.OutputItem));
            if (plan.HasBlockingProblems)
            {
                feedback = new CraftingFeedbackMessage(GetBlockingHeader(plan), "Work Order created and will continue when requirements are met.", true, false, activeWorkOrder.GetRequirementLines(inventory, inventoryCrafting));
            }
            else
            {
                feedback = new CraftingFeedbackMessage("Queue Started", recipe.DisplayName + " x" + Mathf.Clamp(targetQuantity, 1, 9999), false, false, activeWorkOrder.GetRequirementLines(inventory, inventoryCrafting));
            }

            return true;
        }

        public void CancelActiveWorkOrder()
        {
            if (activeWorkOrder == null)
            {
                return;
            }

            activeWorkOrder.Cancel();
            feedback = new CraftingFeedbackMessage("Work Order Canceled", "Unused logical reservations released. Completed intermediates remain in inventory.", false, false, activeWorkOrder.GetRequirementLines(inventory, inventoryCrafting));
            activeWorkOrder = null;
        }

        public void ClearActiveWorkOrder()
        {
            activeWorkOrder = null;
        }

        public bool CanQueueWorkOrder(CraftingRecipe recipe, int targetQuantity)
        {
            if (recipe == null)
            {
                return false;
            }

            EnsurePlanner();
            CraftingPlan plan = planner.BuildPlan(recipe, Mathf.Clamp(targetQuantity, 1, 9999));
            return !plan.HasBlockingProblems;
        }

        public int CountItem(ItemDefinition item)
        {
            return inventory != null ? inventory.CountItem(item) : 0;
        }

        public int CountReservedItem(ItemDefinition item)
        {
            return activeWorkOrder != null ? activeWorkOrder.CountReserved(item) : 0;
        }

        public int CountAvailableItem(ItemDefinition item)
        {
            return Mathf.Max(0, CountItem(item) - CountReservedItem(item));
        }

        public bool HasRequiredStation(CraftingRecipe recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            return recipe.StationType == CraftingStationType.Inventory
                || HasUsablePortFor(recipe);
        }

        public CraftingPortDefinition GetPortForRecipe(CraftingRecipe recipe)
        {
            if (recipe == null || inventoryCrafting == null || recipe.StationType == CraftingStationType.Inventory)
            {
                return null;
            }

            return inventoryCrafting.GetPort(recipe.StationType);
        }

        private bool HasUsablePortFor(CraftingRecipe recipe)
        {
            if (recipe == null || inventoryCrafting == null)
            {
                return false;
            }

            CraftingPortDefinition port = inventoryCrafting.GetPort(recipe.StationType);
            if (port == null || port.PortLevel < recipe.RequiredPortLevel)
            {
                return false;
            }

            if (port.AllowedRecipes.Count == 0)
            {
                return true;
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

        private CraftingRequirementLine GetStationProblemLine(CraftingRecipe recipe)
        {
            if (recipe == null || recipe.StationType == CraftingStationType.Inventory)
            {
                return null;
            }

            CraftingPortDefinition port = inventoryCrafting != null ? inventoryCrafting.GetPort(recipe.StationType) : null;
            if (port == null)
            {
                return new CraftingRequirementLine(null, "Missing " + CraftingTerminology.GetStationDisplayName(recipe.StationType), 0, 1, 0, CraftingRequirementStatus.MissingPort, "Socket " + CraftingTerminology.GetStationDisplayName(recipe.StationType));
            }

            if (port.PortLevel < recipe.RequiredPortLevel)
            {
                return new CraftingRequirementLine(port, port.DisplayName, port.PortLevel, recipe.RequiredPortLevel, 0, CraftingRequirementStatus.InsufficientPortTier, "Upgrade required");
            }

            if (port.AllowedRecipes.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < port.AllowedRecipes.Count; i++)
            {
                if (port.AllowedRecipes[i] == recipe)
                {
                    return null;
                }
            }

            return new CraftingRequirementLine(port, recipe.DisplayName, 0, 1, 0, CraftingRequirementStatus.RecipeLocked, "Recipe locked on " + port.DisplayName);
        }

        public List<CraftingRequirementLine> GetRecipePreview(CraftingRecipe recipe, int targetQuantity)
        {
            return GetRecipePreview(recipe, targetQuantity, false);
        }

        public List<CraftingRequirementLine> GetRecipePreview(CraftingRecipe recipe, int targetQuantity, bool showDetails)
        {
            List<CraftingRequirementLine> lines = new List<CraftingRequirementLine>();
            if (recipe == null)
            {
                return lines;
            }

            EnsurePlanner();
            int craftCount = Mathf.CeilToInt((float)Mathf.Clamp(targetQuantity, 1, 9999) / recipe.OutputQuantity);
            List<CraftingRequirementLine> detailLines = new List<CraftingRequirementLine>();
            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = recipe.Ingredients[i];
                if (ingredient == null || ingredient.Item == null)
                {
                    continue;
                }

                int required = ingredient.Quantity * craftCount;
                int totalOwned = CountItem(ingredient.Item);
                int reserved = CountReservedItem(ingredient.Item);
                int available = CountAvailableItem(ingredient.Item);
                CraftingRequirementStatus status = available >= required
                    ? (reserved > 0 ? CraftingRequirementStatus.Reserved : CraftingRequirementStatus.Satisfied)
                    : CraftingRequirementStatus.Missing;
                string detail = available >= required
                    ? (reserved > 0 ? "Available " + available + " of " + totalOwned + " total" : "Ready")
                    : "Missing " + Mathf.Max(0, required - available) + " available";

                if (available < required)
                {
                    CraftingRecipe producer = planner.FindRecipeFor(ingredient.Item);
                    if (producer != null)
                    {
                        CraftingPlan subPlan = planner.BuildPlan(producer, required - available);
                        if (!subPlan.HasBlockingProblems)
                        {
                            status = CraftingRequirementStatus.Queueable;
                            detail = "Can process from " + DescribeIngredients(producer);
                        }
                        else
                        {
                            detail = DescribePlanProblem(subPlan);
                        }

                        AddProducerDetailLines(detailLines, producer, Mathf.Max(0, required - available), showDetails);
                    }
                }

                lines.Add(new CraftingRequirementLine(ingredient.Item, ingredient.Item.DisplayName, available, required, reserved, status, detail));
            }

            CraftingRequirementLine stationProblem = GetStationProblemLine(recipe);
            if (stationProblem != null)
            {
                lines.Add(stationProblem);
            }

            if (showDetails)
            {
                lines.AddRange(detailLines);
            }

            return lines;
        }

        public List<CraftingRequirementLine> GetActiveWorkOrderLines()
        {
            return activeWorkOrder != null ? activeWorkOrder.GetRequirementLines(inventory, inventoryCrafting) : new List<CraftingRequirementLine>();
        }

        private void EnsurePlanner()
        {
            if (planner == null)
            {
                planner = new CraftingPlanner(recipes, inventory, inventoryCrafting);
            }
        }

        private void PlayCompletionCueIfNeeded()
        {
            if (activeWorkOrder == null || !activeWorkOrder.IsComplete)
            {
                return;
            }

            if (activeWorkOrder.CompletionSound != null)
            {
                AudioSource.PlayClipAtPoint(activeWorkOrder.CompletionSound, transform.position);
                return;
            }

            if (!string.IsNullOrEmpty(activeWorkOrder.CompletionCue))
            {
                Debug.Log(activeWorkOrder.CompletionCue);
            }
        }

        private List<CraftingRequirementLine> BuildPlanLines(CraftingPlan plan)
        {
            List<CraftingRequirementLine> lines = new List<CraftingRequirementLine>();
            if (plan == null)
            {
                return lines;
            }

            foreach (KeyValuePair<ItemDefinition, int> pair in plan.Requirements)
            {
                ItemDefinition item = pair.Key;
                int available = CountAvailableItem(item);
                CraftingRequirementStatus status = available >= pair.Value ? CraftingRequirementStatus.Satisfied : CraftingRequirementStatus.Missing;
                lines.Add(new CraftingRequirementLine(item, item != null ? item.DisplayName : "Item", available, pair.Value, plan.GetReserved(item), status, available >= pair.Value ? "Ready" : "Missing " + (pair.Value - available)));
            }

            foreach (KeyValuePair<ItemDefinition, int> pair in plan.MissingMaterials)
            {
                ItemDefinition item = pair.Key;
                lines.Add(new CraftingRequirementLine(item, item != null ? item.DisplayName : "Item", CountItem(item), pair.Value, 0, CraftingRequirementStatus.Missing, "Missing " + pair.Value));
            }

            for (int i = 0; i < plan.BlockingMessages.Count; i++)
            {
                lines.Add(new CraftingRequirementLine(null, plan.BlockingMessages[i], 0, 1, 0, CraftingRequirementStatus.Missing, plan.BlockingMessages[i]));
            }

            return lines;
        }

        private static string GetBlockingHeader(CraftingPlan plan)
        {
            if (plan == null)
            {
                return "Insufficient Materials";
            }

            for (int i = 0; i < plan.BlockingMessages.Count; i++)
            {
                string message = plan.BlockingMessages[i];
                if (message.Contains("Missing Crafting Trinket"))
                {
                    return "Missing Crafting Trinket";
                }
                if (message.Contains("Insufficient Crafting Trinket Tier"))
                {
                    return "Insufficient Crafting Trinket Tier";
                }
                if (message.Contains("Recipe Locked"))
                {
                    return "Recipe Locked";
                }
            }

            if (plan.MissingMaterials.Count > 0)
            {
                return "Insufficient Materials";
            }

            return "Insufficient Materials";
        }

        private static bool HasStationBlockingProblem(CraftingPlan plan)
        {
            if (plan == null)
            {
                return false;
            }

            for (int i = 0; i < plan.BlockingMessages.Count; i++)
            {
                string message = plan.BlockingMessages[i];
                if (IsStationBlockingMessage(message))
                {
                    return true;
                }
            }

            return false;
        }

        private static string BuildStationBlockingDetail(CraftingPlan plan)
        {
            if (plan == null || plan.FinalRecipe == null)
            {
                return "Socket the required crafting trinket before starting this Work Order.";
            }

            for (int i = 0; i < plan.BlockingMessages.Count; i++)
            {
                string message = plan.BlockingMessages[i];
                if (message.Contains("Missing Crafting Trinket"))
                {
                    string socketInstruction = message.Replace("Missing Crafting Trinket: socket ", string.Empty).TrimEnd('.');
                    return "Socket " + socketInstruction + " before starting " + plan.FinalRecipe.DisplayName + ".";
                }

                if (message.Contains("Insufficient Crafting Trinket Tier"))
                {
                    return "Upgrade the socketed crafting trinket before starting " + plan.FinalRecipe.DisplayName + ".";
                }

                if (message.Contains("Recipe Locked"))
                {
                    return "Socket a crafting trinket that allows " + plan.FinalRecipe.DisplayName + ".";
                }
            }

            return "Socket the required crafting trinket before starting this Work Order.";
        }

        private static bool IsStationBlockingMessage(string message)
        {
            return message != null
                && (message.Contains("Missing Crafting Trinket")
                    || message.Contains("Insufficient Crafting Trinket Tier")
                    || message.Contains(" is not allowed by "));
        }

        private static string DescribePlanProblem(CraftingPlan plan)
        {
            if (plan == null)
            {
                return "Cannot plan dependency";
            }

            foreach (KeyValuePair<ItemDefinition, int> pair in plan.MissingMaterials)
            {
                return "Missing " + (pair.Key != null ? pair.Key.DisplayName : "Item") + " x" + pair.Value;
            }

            if (plan.BlockingMessages.Count > 0)
            {
                return plan.BlockingMessages[0];
            }

            return "Cannot process dependency";
        }

        private static string DescribeIngredients(CraftingRecipe recipe)
        {
            if (recipe == null)
            {
                return "known recipe";
            }

            List<string> parts = new List<string>();
            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = recipe.Ingredients[i];
                if (ingredient != null && ingredient.Item != null)
                {
                    parts.Add(ingredient.Item.DisplayName);
                }
            }

            return parts.Count > 0 ? string.Join(", ", parts.ToArray()) : "known recipe";
        }

        private void AddProducerDetailLines(List<CraftingRequirementLine> lines, CraftingRecipe producer, int outputShortage, bool includeNestedProducers)
        {
            if (lines == null || producer == null || outputShortage <= 0)
            {
                return;
            }

            int craftCount = Mathf.CeilToInt((float)outputShortage / producer.OutputQuantity);
            for (int i = 0; i < producer.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = producer.Ingredients[i];
                if (ingredient == null || ingredient.Item == null)
                {
                    continue;
                }

                int required = ingredient.Quantity * craftCount;
                int totalOwned = CountItem(ingredient.Item);
                int reserved = CountReservedItem(ingredient.Item);
                int available = CountAvailableItem(ingredient.Item);
                CraftingRequirementStatus status = available >= required
                    ? (reserved > 0 ? CraftingRequirementStatus.Reserved : CraftingRequirementStatus.Satisfied)
                    : CraftingRequirementStatus.Missing;
                string outputName = producer.OutputItem != null ? producer.OutputItem.DisplayName : producer.DisplayName;
                string detail = available >= required
                    ? "Needed for " + outputName
                    : "Missing " + Mathf.Max(0, required - available) + " available for " + outputName;

                lines.Add(new CraftingRequirementLine(
                    ingredient.Item,
                    "  " + ingredient.Item.DisplayName,
                    available,
                    required,
                    reserved,
                    status,
                    detail + " (owned " + totalOwned + ")"));

                if (includeNestedProducers && available < required)
                {
                    CraftingRecipe nestedProducer = planner.FindRecipeFor(ingredient.Item);
                    if (nestedProducer != null && nestedProducer != producer)
                    {
                        AddProducerDetailLines(lines, nestedProducer, required - available, true);
                    }
                }
            }

            CraftingRequirementLine stationProblem = GetStationProblemLine(producer);
            if (stationProblem != null)
            {
                lines.Add(stationProblem);
            }
        }

        private void ValidateRecipeDataOnce()
        {
            if (validatedRecipeData)
            {
                return;
            }

            validatedRecipeData = true;
            Dictionary<ItemDefinition, CraftingRecipe> outputs = new Dictionary<ItemDefinition, CraftingRecipe>();
            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe recipe = recipes[i];
                if (recipe == null)
                {
                    Debug.LogWarning("Crafting recipe list contains an empty entry.");
                    continue;
                }

                if (recipe.OutputItem == null)
                {
                    Debug.LogWarning("Crafting recipe '" + recipe.DisplayName + "' has no output item.");
                }
                else
                {
                    CraftingRecipe existing;
                    if (outputs.TryGetValue(recipe.OutputItem, out existing))
                    {
                        Debug.LogWarning("Crafting recipes '" + existing.DisplayName + "' and '" + recipe.DisplayName + "' both output " + recipe.OutputItem.DisplayName + ".");
                    }
                    else
                    {
                        outputs.Add(recipe.OutputItem, recipe);
                    }
                }

                if (recipe.OutputQuantity <= 0)
                {
                    Debug.LogWarning("Crafting recipe '" + recipe.DisplayName + "' has an invalid output quantity.");
                }

                HashSet<ItemDefinition> seenIngredients = new HashSet<ItemDefinition>();
                for (int ingredientIndex = 0; ingredientIndex < recipe.Ingredients.Count; ingredientIndex++)
                {
                    CraftingIngredient ingredient = recipe.Ingredients[ingredientIndex];
                    if (ingredient == null || ingredient.Item == null)
                    {
                        Debug.LogWarning("Crafting recipe '" + recipe.DisplayName + "' has an empty ingredient slot.");
                        continue;
                    }

                    if (ingredient.Quantity <= 0)
                    {
                        Debug.LogWarning("Crafting recipe '" + recipe.DisplayName + "' has an invalid quantity for " + ingredient.Item.DisplayName + ".");
                    }

                    if (!seenIngredients.Add(ingredient.Item))
                    {
                        Debug.LogWarning("Crafting recipe '" + recipe.DisplayName + "' lists " + ingredient.Item.DisplayName + " more than once.");
                    }
                }
            }
        }
    }
}
