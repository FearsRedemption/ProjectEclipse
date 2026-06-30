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
        [SerializeField] private int maxQueuedWorkOrders = 10;

        private InventoryStore inventory;
        private EquipmentController equipment;
        private InventoryCraftingController inventoryCrafting;
        private CraftingPlanner planner;
        private readonly List<WorkOrder> workOrders = new List<WorkOrder>();
        private CraftingFeedbackMessage feedback;
        private bool validatedRecipeData;

        public IReadOnlyList<CraftingRecipe> Recipes { get { return recipes; } }
        public IReadOnlyList<WorkOrder> WorkOrders { get { return workOrders; } }
        public WorkOrder ActiveWorkOrder { get { return GetFirstQueuedWorkOrder(); } }
        public int MaxQueuedWorkOrders { get { return Mathf.Max(1, maxQueuedWorkOrders); } }
        public int QueuedWorkOrderCount { get { return workOrders.Count; } }
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
            WorkOrder processingOrder = GetNextProcessingWorkOrder();
            if (processingOrder == null)
            {
                return;
            }

            CraftingFeedbackMessage workOrderFeedback;
            processingOrder.Tick(inventory, inventoryCrafting, equipment, out workOrderFeedback);
            if (workOrderFeedback != null)
            {
                feedback = workOrderFeedback;
                PlayCompletionCueIfNeeded(processingOrder);
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

            if (workOrders.Count >= MaxQueuedWorkOrders)
            {
                feedback = new CraftingFeedbackMessage("Work Order Queue Full", "Finish, dismiss, or cancel a Work Order before adding another.", true, false, null);
                return false;
            }

            int clampedTargetQuantity = Mathf.Clamp(targetQuantity, 1, 9999);
            EnsurePlanner();
            CraftingPlan plan = BuildPlanWithReservations(recipe, clampedTargetQuantity, BuildReservedInventorySnapshot(null));
            if (plan.HasLoopError)
            {
                feedback = new CraftingFeedbackMessage("Recipe Locked", "Dependency loop detected. Work Order was not started.", true, false, BuildPlanLines(plan));
                return false;
            }

            if (HasStationBlockingProblem(plan))
            {
                bool prerequisiteHandled;
                bool queuedWithPrerequisites = TryQueueWithRequiredTrinkets(recipe, clampedTargetQuantity, plan, out prerequisiteHandled);
                if (prerequisiteHandled)
                {
                    return queuedWithPrerequisites;
                }

                WorkOrder previewOrder = new WorkOrder(plan, inventory.CountItem(recipe.OutputItem));
                feedback = new CraftingFeedbackMessage(
                    GetBlockingHeader(plan),
                    BuildStationBlockingDetail(plan),
                    true,
                    false,
                    previewOrder.GetRequirementLines(inventory, inventoryCrafting, BuildReservedInventorySnapshot(null)));
                return false;
            }

            WorkOrder order = new WorkOrder(plan, inventory.CountItem(recipe.OutputItem));
            workOrders.Add(order);
            if (plan.HasBlockingProblems)
            {
                feedback = new CraftingFeedbackMessage(GetBlockingHeader(plan), "Work Order queued and will continue when requirements are met.", true, false, GetWorkOrderLines(order));
            }
            else
            {
                feedback = new CraftingFeedbackMessage("Work Order Queued", "WO" + workOrders.Count + ": " + recipe.DisplayName + " x" + clampedTargetQuantity, false, false, GetWorkOrderLines(order));
            }

            return true;
        }

        public void CancelActiveWorkOrder()
        {
            CancelWorkOrder(ActiveWorkOrder);
        }

        public void ClearActiveWorkOrder()
        {
            ClearWorkOrder(ActiveWorkOrder);
        }

        public void CancelWorkOrder(WorkOrder order)
        {
            if (order == null)
            {
                return;
            }

            order.Cancel();
            feedback = new CraftingFeedbackMessage("Work Order Canceled", "Unused logical reservations released. Completed intermediates remain in inventory.", false, false, GetWorkOrderLines(order));
            workOrders.Remove(order);
        }

        public void ClearWorkOrder(WorkOrder order)
        {
            if (order != null)
            {
                workOrders.Remove(order);
            }
        }

        public bool CanQueueWorkOrder(CraftingRecipe recipe, int targetQuantity)
        {
            if (recipe == null || workOrders.Count >= MaxQueuedWorkOrders)
            {
                return false;
            }

            EnsurePlanner();
            CraftingPlan plan = BuildPlanWithReservations(recipe, Mathf.Clamp(targetQuantity, 1, 9999), BuildReservedInventorySnapshot(null));
            if (!plan.HasBlockingProblems)
            {
                return true;
            }

            if (plan.HasLoopError || plan.MissingMaterials.Count > 0 || !HasStationBlockingProblem(plan) || HasUncoveredStationBlockingProblem(plan))
            {
                return false;
            }

            List<CraftingRecipe> prerequisiteRecipes = new List<CraftingRecipe>();
            string failureReason;
            return TryCollectMissingCraftingTrinketRecipes(plan, prerequisiteRecipes, out failureReason)
                && string.IsNullOrEmpty(failureReason)
                && workOrders.Count + prerequisiteRecipes.Count + 1 <= MaxQueuedWorkOrders;
        }

        public int CountItem(ItemDefinition item)
        {
            return inventory != null ? inventory.CountItem(item) : 0;
        }

        public int CountReservedItem(ItemDefinition item)
        {
            if (item == null)
            {
                return 0;
            }

            int total = 0;
            for (int i = 0; i < workOrders.Count; i++)
            {
                if (workOrders[i] != null && !workOrders[i].IsCanceled)
                {
                    total += workOrders[i].CountReserved(item);
                }
            }

            return total;
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
            return port != null && port.CanCraft(recipe);
        }

        private bool TryQueueWithRequiredTrinkets(CraftingRecipe recipe, int targetQuantity, CraftingPlan blockedPlan, out bool handled)
        {
            handled = false;
            if (recipe == null || inventoryCrafting == null)
            {
                return false;
            }

            List<CraftingRecipe> prerequisiteRecipes = new List<CraftingRecipe>();
            string failureReason;
            if (!TryCollectMissingCraftingTrinketRecipes(blockedPlan, prerequisiteRecipes, out failureReason))
            {
                if (!string.IsNullOrEmpty(failureReason))
                {
                    handled = true;
                    feedback = new CraftingFeedbackMessage("Missing Crafting Trinket", failureReason, true, false, BuildPlanLines(blockedPlan));
                }

                return false;
            }

            handled = true;
            if (workOrders.Count + prerequisiteRecipes.Count + 1 > MaxQueuedWorkOrders)
            {
                feedback = new CraftingFeedbackMessage("Work Order Queue Full", "Make room for the required trinket Work Order before adding this craft.", true, false, null);
                return false;
            }

            List<WorkOrder> addedPrerequisites = new List<WorkOrder>();
            for (int i = 0; i < prerequisiteRecipes.Count; i++)
            {
                CraftingRecipe prerequisiteRecipe = prerequisiteRecipes[i];
                CraftingPlan prerequisitePlan = BuildPlanWithReservations(prerequisiteRecipe, prerequisiteRecipe.OutputQuantity, BuildReservedInventorySnapshot(null));
                if (prerequisitePlan.HasLoopError || HasStationBlockingProblem(prerequisitePlan))
                {
                    RemoveQueuedOrders(addedPrerequisites);
                    feedback = new CraftingFeedbackMessage("Recipe Locked", "The required trinket cannot be queued yet.", true, false, BuildPlanLines(prerequisitePlan));
                    return false;
                }

                WorkOrder prerequisiteOrder = new WorkOrder(prerequisitePlan, inventory.CountItem(prerequisiteRecipe.OutputItem));
                workOrders.Add(prerequisiteOrder);
                addedPrerequisites.Add(prerequisiteOrder);
            }

            CraftingPlan finalPlan = BuildPlanWithReservations(recipe, targetQuantity, BuildReservedInventorySnapshot(null));
            if (finalPlan.HasLoopError)
            {
                RemoveQueuedOrders(addedPrerequisites);
                feedback = new CraftingFeedbackMessage("Recipe Locked", "Dependency loop detected. Work Order was not started.", true, false, BuildPlanLines(finalPlan));
                return false;
            }

            if (HasUncoveredStationBlockingProblem(finalPlan))
            {
                RemoveQueuedOrders(addedPrerequisites);
                WorkOrder previewOrder = new WorkOrder(finalPlan, inventory.CountItem(recipe.OutputItem));
                feedback = new CraftingFeedbackMessage(
                    GetBlockingHeader(finalPlan),
                    BuildStationBlockingDetail(finalPlan),
                    true,
                    false,
                    previewOrder.GetRequirementLines(inventory, inventoryCrafting, BuildReservedInventorySnapshot(null)));
                return false;
            }

            WorkOrder finalOrder = new WorkOrder(finalPlan, inventory.CountItem(recipe.OutputItem));
            workOrders.Add(finalOrder);
            feedback = new CraftingFeedbackMessage(
                "Work Orders Queued",
                BuildTrinketQueueDetail(prerequisiteRecipes, recipe),
                false,
                false,
                GetWorkOrderLines(finalOrder));
            return true;
        }

        private bool TryCollectMissingCraftingTrinketRecipes(CraftingPlan plan, List<CraftingRecipe> prerequisiteRecipes, out string failureReason)
        {
            failureReason = string.Empty;
            if (plan == null || prerequisiteRecipes == null)
            {
                return false;
            }

            bool foundMissingTrinket = false;
            for (int i = 0; i < plan.Steps.Count; i++)
            {
                CraftingPlanStep step = plan.Steps[i];
                CraftingRecipe stepRecipe = step != null ? step.Recipe : null;
                if (stepRecipe == null || stepRecipe.StationType == CraftingStationType.Inventory)
                {
                    continue;
                }

                CraftingPortDefinition currentPort = inventoryCrafting != null ? inventoryCrafting.GetPort(stepRecipe.StationType) : null;
                if (currentPort != null)
                {
                    continue;
                }

                foundMissingTrinket = true;
                if (HasQueuedUsablePortFor(stepRecipe, prerequisiteRecipes))
                {
                    continue;
                }

                CraftingRecipe trinketRecipe = FindCraftingPortRecipeFor(stepRecipe);
                if (trinketRecipe == null)
                {
                    failureReason = "No recipe is available for " + CraftingTerminology.GetStationDisplayName(stepRecipe.StationType) + ".";
                    return false;
                }

                if (stepRecipe.OutputItem != null && RecipeDependencyChainUsesItem(trinketRecipe, stepRecipe.OutputItem, BuildRecipeOutputMap(), new HashSet<CraftingRecipe>()))
                {
                    failureReason = "Recipe cross-lap detected: " + stepRecipe.DisplayName + " needs " + CraftingTerminology.GetStationDisplayName(stepRecipe.StationType) + ", but " + trinketRecipe.DisplayName + " needs " + stepRecipe.OutputItem.DisplayName + ".";
                    return false;
                }

                if (!prerequisiteRecipes.Contains(trinketRecipe))
                {
                    prerequisiteRecipes.Add(trinketRecipe);
                }
            }

            return foundMissingTrinket;
        }

        private CraftingRecipe FindCraftingPortRecipeFor(CraftingRecipe requiredRecipe)
        {
            CraftingRecipe bestRecipe = null;
            CraftingRecipe fallbackCrosslapRecipe = null;
            int bestPortLevel = int.MaxValue;
            int fallbackPortLevel = int.MaxValue;
            Dictionary<ItemDefinition, CraftingRecipe> outputRecipes = BuildRecipeOutputMap();
            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe candidate = recipes[i];
                CraftingPortDefinition port = candidate != null ? candidate.OutputItem as CraftingPortDefinition : null;
                if (port == null || !RecipeOutputsUsablePort(candidate, requiredRecipe))
                {
                    continue;
                }

                if (requiredRecipe != null
                    && requiredRecipe.OutputItem != null
                    && RecipeDependencyChainUsesItem(candidate, requiredRecipe.OutputItem, outputRecipes, new HashSet<CraftingRecipe>()))
                {
                    if (port.PortLevel < fallbackPortLevel)
                    {
                        fallbackCrosslapRecipe = candidate;
                        fallbackPortLevel = port.PortLevel;
                    }

                    continue;
                }

                if (port.PortLevel < bestPortLevel)
                {
                    bestRecipe = candidate;
                    bestPortLevel = port.PortLevel;
                }
            }

            return bestRecipe != null ? bestRecipe : fallbackCrosslapRecipe;
        }

        private bool HasQueuedUsablePortFor(CraftingRecipe requiredRecipe, List<CraftingRecipe> pendingPrerequisites)
        {
            if (requiredRecipe == null)
            {
                return false;
            }

            if (pendingPrerequisites != null)
            {
                for (int i = 0; i < pendingPrerequisites.Count; i++)
                {
                    if (RecipeOutputsUsablePort(pendingPrerequisites[i], requiredRecipe))
                    {
                        return true;
                    }
                }
            }

            for (int i = 0; i < workOrders.Count; i++)
            {
                WorkOrder order = workOrders[i];
                CraftingRecipe queuedRecipe = order != null && order.Plan != null ? order.Plan.FinalRecipe : null;
                if (order != null && !order.IsCanceled && RecipeOutputsUsablePort(queuedRecipe, requiredRecipe))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool RecipeOutputsUsablePort(CraftingRecipe portRecipe, CraftingRecipe requiredRecipe)
        {
            CraftingPortDefinition port = portRecipe != null ? portRecipe.OutputItem as CraftingPortDefinition : null;
            if (port == null || requiredRecipe == null)
            {
                return false;
            }

            return port.CanCraft(requiredRecipe);
        }

        private void RemoveQueuedOrders(List<WorkOrder> orders)
        {
            if (orders == null)
            {
                return;
            }

            for (int i = 0; i < orders.Count; i++)
            {
                workOrders.Remove(orders[i]);
            }
        }

        private Dictionary<ItemDefinition, CraftingRecipe> BuildRecipeOutputMap()
        {
            Dictionary<ItemDefinition, CraftingRecipe> outputs = new Dictionary<ItemDefinition, CraftingRecipe>();
            if (recipes == null)
            {
                return outputs;
            }

            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe recipe = recipes[i];
                if (recipe != null && recipe.OutputItem != null && !outputs.ContainsKey(recipe.OutputItem))
                {
                    outputs.Add(recipe.OutputItem, recipe);
                }
            }

            return outputs;
        }

        private static bool RecipeDependencyChainUsesItem(
            CraftingRecipe recipe,
            ItemDefinition targetItem,
            IReadOnlyDictionary<ItemDefinition, CraftingRecipe> outputRecipes,
            HashSet<CraftingRecipe> visitedRecipes)
        {
            if (recipe == null || targetItem == null || visitedRecipes == null || !visitedRecipes.Add(recipe))
            {
                return false;
            }

            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = recipe.Ingredients[i];
                if (ingredient == null || ingredient.Item == null)
                {
                    continue;
                }

                if (ingredient.Item == targetItem)
                {
                    return true;
                }

                CraftingRecipe producer;
                if (outputRecipes != null
                    && outputRecipes.TryGetValue(ingredient.Item, out producer)
                    && RecipeDependencyChainUsesItem(producer, targetItem, outputRecipes, visitedRecipes))
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

            return port.CanCraft(recipe)
                ? null
                : new CraftingRequirementLine(port, recipe.DisplayName, 0, 1, 0, CraftingRequirementStatus.RecipeLocked, "Recipe locked on " + port.DisplayName);
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
                        CraftingPlan subPlan = BuildPlanWithReservations(producer, required - available, BuildReservedInventorySnapshot(null));
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
            return GetWorkOrderLines(ActiveWorkOrder);
        }

        public List<CraftingRequirementLine> GetWorkOrderLines(WorkOrder order)
        {
            return order != null
                ? order.GetRequirementLines(inventory, inventoryCrafting, BuildReservedInventorySnapshot(order))
                : new List<CraftingRequirementLine>();
        }

        private void EnsurePlanner()
        {
            if (planner == null)
            {
                planner = new CraftingPlanner(recipes, inventory, inventoryCrafting);
            }
        }

        private CraftingPlan BuildPlanWithReservations(
            CraftingRecipe recipe,
            int targetQuantity,
            IReadOnlyDictionary<ItemDefinition, int> reservedInventory)
        {
            CraftingPlanner reservationAwarePlanner = new CraftingPlanner(recipes, inventory, inventoryCrafting, reservedInventory);
            return reservationAwarePlanner.BuildPlan(recipe, targetQuantity);
        }

        private Dictionary<ItemDefinition, int> BuildReservedInventorySnapshot(WorkOrder stopBefore)
        {
            Dictionary<ItemDefinition, int> reserved = new Dictionary<ItemDefinition, int>();
            for (int i = 0; i < workOrders.Count; i++)
            {
                WorkOrder order = workOrders[i];
                if (order == stopBefore)
                {
                    break;
                }

                if (order == null || order.IsCanceled || order.Plan == null)
                {
                    continue;
                }

                foreach (KeyValuePair<ItemDefinition, int> pair in order.Plan.Reservations)
                {
                    int quantity = order.CountReserved(pair.Key);
                    if (pair.Key == null || quantity <= 0)
                    {
                        continue;
                    }

                    int existing;
                    reserved.TryGetValue(pair.Key, out existing);
                    reserved[pair.Key] = existing + quantity;
                }
            }

            return reserved;
        }

        private WorkOrder GetFirstQueuedWorkOrder()
        {
            for (int i = 0; i < workOrders.Count; i++)
            {
                if (workOrders[i] != null && !workOrders[i].IsCanceled)
                {
                    return workOrders[i];
                }
            }

            return null;
        }

        private WorkOrder GetNextProcessingWorkOrder()
        {
            for (int i = 0; i < workOrders.Count; i++)
            {
                WorkOrder order = workOrders[i];
                if (order != null && !order.IsCanceled && !order.IsComplete)
                {
                    return order;
                }
            }

            return null;
        }

        private void PlayCompletionCueIfNeeded(WorkOrder order)
        {
            if (order == null || !order.IsComplete)
            {
                return;
            }

            if (order.CompletionSound != null)
            {
                AudioSource.PlayClipAtPoint(order.CompletionSound, transform.position);
                return;
            }

            if (!string.IsNullOrEmpty(order.CompletionCue))
            {
                Debug.Log(order.CompletionCue);
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

        private static bool HasUncoveredStationBlockingProblem(CraftingPlan plan)
        {
            if (plan == null)
            {
                return false;
            }

            for (int i = 0; i < plan.BlockingMessages.Count; i++)
            {
                string message = plan.BlockingMessages[i];
                if (!IsStationBlockingMessage(message))
                {
                    continue;
                }

                if (message.Contains("Missing Crafting Trinket"))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private static string BuildTrinketQueueDetail(List<CraftingRecipe> prerequisiteRecipes, CraftingRecipe finalRecipe)
        {
            string finalName = finalRecipe != null ? finalRecipe.DisplayName : "the requested craft";
            if (prerequisiteRecipes == null || prerequisiteRecipes.Count == 0)
            {
                return finalName + " queued behind an existing trinket Work Order.";
            }

            List<string> names = new List<string>();
            for (int i = 0; i < prerequisiteRecipes.Count; i++)
            {
                if (prerequisiteRecipes[i] != null)
                {
                    names.Add(prerequisiteRecipes[i].DisplayName);
                }
            }

            string trinketNames = names.Count > 0 ? string.Join(", ", names.ToArray()) : "required trinket";
            return "Queued " + trinketNames + " before " + finalName + ".";
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

            ValidateIngredientCrosslaps(outputs);
            ValidateStationBootstrapCrosslaps(outputs);
            ValidateCraftingPortRecipeCompatibility();
        }

        private void ValidateIngredientCrosslaps(IReadOnlyDictionary<ItemDefinition, CraftingRecipe> outputRecipes)
        {
            HashSet<CraftingRecipe> visiting = new HashSet<CraftingRecipe>();
            HashSet<CraftingRecipe> visited = new HashSet<CraftingRecipe>();
            HashSet<string> loggedCycles = new HashSet<string>();
            List<CraftingRecipe> stack = new List<CraftingRecipe>();
            for (int i = 0; i < recipes.Count; i++)
            {
                VisitIngredientCrosslap(recipes[i], outputRecipes, visiting, visited, stack, loggedCycles);
            }
        }

        private static void VisitIngredientCrosslap(
            CraftingRecipe recipe,
            IReadOnlyDictionary<ItemDefinition, CraftingRecipe> outputRecipes,
            HashSet<CraftingRecipe> visiting,
            HashSet<CraftingRecipe> visited,
            List<CraftingRecipe> stack,
            HashSet<string> loggedCycles)
        {
            if (recipe == null || visited.Contains(recipe))
            {
                return;
            }

            if (visiting.Contains(recipe))
            {
                string cycle = BuildRecipeCyclePath(stack, recipe);
                if (loggedCycles.Add(cycle))
                {
                    Debug.LogError("Crafting recipe cross-lap detected: " + cycle + ".");
                }
                return;
            }

            visiting.Add(recipe);
            stack.Add(recipe);
            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = recipe.Ingredients[i];
                CraftingRecipe producer;
                if (ingredient != null
                    && ingredient.Item != null
                    && outputRecipes != null
                    && outputRecipes.TryGetValue(ingredient.Item, out producer))
                {
                    VisitIngredientCrosslap(producer, outputRecipes, visiting, visited, stack, loggedCycles);
                }
            }

            stack.RemoveAt(stack.Count - 1);
            visiting.Remove(recipe);
            visited.Add(recipe);
        }

        private void ValidateStationBootstrapCrosslaps(IReadOnlyDictionary<ItemDefinition, CraftingRecipe> outputRecipes)
        {
            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe recipe = recipes[i];
                if (recipe == null || recipe.OutputItem == null || recipe.StationType == CraftingStationType.Inventory)
                {
                    continue;
                }

                List<CraftingRecipe> providers = FindCraftingPortRecipesForValidation(recipe);
                if (providers.Count == 0)
                {
                    continue;
                }

                bool hasCleanProvider = false;
                List<string> crosslappingProviders = new List<string>();
                for (int providerIndex = 0; providerIndex < providers.Count; providerIndex++)
                {
                    CraftingRecipe provider = providers[providerIndex];
                    if (RecipeDependencyChainUsesItem(provider, recipe.OutputItem, outputRecipes, new HashSet<CraftingRecipe>()))
                    {
                        crosslappingProviders.Add(provider.DisplayName);
                    }
                    else
                    {
                        hasCleanProvider = true;
                    }
                }

                if (!hasCleanProvider)
                {
                    Debug.LogError(
                        "Crafting station cross-lap detected: " + recipe.DisplayName +
                        " needs " + CraftingTerminology.GetStationDisplayName(recipe.StationType) +
                        ", but every available trinket recipe for that station needs " + recipe.OutputItem.DisplayName +
                        " (" + string.Join(", ", crosslappingProviders.ToArray()) + ").");
                }
            }
        }

        private List<CraftingRecipe> FindCraftingPortRecipesForValidation(CraftingRecipe requiredRecipe)
        {
            List<CraftingRecipe> providers = new List<CraftingRecipe>();
            if (requiredRecipe == null)
            {
                return providers;
            }

            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe candidate = recipes[i];
                if (candidate != null && RecipeOutputsUsablePort(candidate, requiredRecipe))
                {
                    providers.Add(candidate);
                }
            }

            return providers;
        }

        private void ValidateCraftingPortRecipeCompatibility()
        {
            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe recipe = recipes[i];
                CraftingPortDefinition port = recipe != null ? recipe.OutputItem as CraftingPortDefinition : null;
                if (port == null)
                {
                    continue;
                }

                IReadOnlyList<CraftingRecipe> allowedRecipes = port.AllowedRecipes;
                if (allowedRecipes == null)
                {
                    continue;
                }

                for (int allowedIndex = 0; allowedIndex < allowedRecipes.Count; allowedIndex++)
                {
                    CraftingRecipe allowedRecipe = allowedRecipes[allowedIndex];
                    if (allowedRecipe == null)
                    {
                        Debug.LogWarning("Crafting trinket '" + port.DisplayName + "' has an empty allowed recipe slot.");
                        continue;
                    }

                    if (allowedRecipe.StationType != port.StationType)
                    {
                        Debug.LogWarning("Crafting trinket '" + port.DisplayName + "' allows '" + allowedRecipe.DisplayName + "', but their station types do not match.");
                    }

                    if (allowedRecipe.RequiredPortLevel > port.PortLevel)
                    {
                        Debug.LogWarning("Crafting trinket '" + port.DisplayName + "' allows '" + allowedRecipe.DisplayName + "', but the recipe requires a higher trinket tier.");
                    }
                }
            }
        }

        private static string BuildRecipeCyclePath(List<CraftingRecipe> stack, CraftingRecipe repeatedRecipe)
        {
            List<string> names = new List<string>();
            int startIndex = stack != null ? stack.IndexOf(repeatedRecipe) : -1;
            if (stack != null && startIndex >= 0)
            {
                for (int i = startIndex; i < stack.Count; i++)
                {
                    names.Add(stack[i] != null ? stack[i].DisplayName : "Recipe");
                }
            }
            else if (repeatedRecipe != null)
            {
                names.Add(repeatedRecipe.DisplayName);
            }

            names.Add(repeatedRecipe != null ? repeatedRecipe.DisplayName : "Recipe");
            return string.Join(" -> ", names.ToArray());
        }
    }
}
