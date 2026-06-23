using System.Collections.Generic;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Crafting
{
    public class CraftingPlanner
    {
        private readonly IReadOnlyList<CraftingRecipe> recipes;
        private readonly InventoryStore inventory;
        private readonly InventoryCraftingController inventoryCrafting;

        public CraftingPlanner(
            IReadOnlyList<CraftingRecipe> recipes,
            InventoryStore inventory,
            InventoryCraftingController inventoryCrafting)
        {
            this.recipes = recipes;
            this.inventory = inventory;
            this.inventoryCrafting = inventoryCrafting;
        }

        public CraftingPlan BuildPlan(CraftingRecipe finalRecipe, int targetQuantity)
        {
            CraftingPlan plan = new CraftingPlan(finalRecipe, targetQuantity);
            if (finalRecipe == null || finalRecipe.OutputItem == null)
            {
                plan.AddBlockingMessage("Recipe Locked: recipe data is incomplete.");
                return plan;
            }

            Dictionary<ItemDefinition, int> available = SnapshotInventory();
            int craftCount = Mathf.CeilToInt((float)Mathf.Max(1, targetQuantity) / finalRecipe.OutputQuantity);
            ResolveRecipe(finalRecipe, craftCount, true, available, plan, new List<CraftingRecipe>());
            return plan;
        }

        public CraftingRecipe FindRecipeFor(ItemDefinition outputItem)
        {
            if (outputItem == null || recipes == null)
            {
                return null;
            }

            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe recipe = recipes[i];
                if (recipe != null && recipe.OutputItem == outputItem)
                {
                    return recipe;
                }
            }

            return null;
        }

        private void ResolveRecipe(
            CraftingRecipe recipe,
            int craftCount,
            bool isFinal,
            Dictionary<ItemDefinition, int> available,
            CraftingPlan plan,
            List<CraftingRecipe> stack)
        {
            if (recipe == null)
            {
                plan.AddBlockingMessage("Recipe Locked: missing recipe.");
                return;
            }

            if (stack.Contains(recipe))
            {
                plan.AddLoopError("Recipe Locked: dependency loop at " + recipe.DisplayName + ".");
                return;
            }

            stack.Add(recipe);
            CheckStation(recipe, plan);

            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = recipe.Ingredients[i];
                if (ingredient == null || ingredient.Item == null)
                {
                    continue;
                }

                ResolveItem(ingredient.Item, ingredient.Quantity * Mathf.Max(1, craftCount), available, plan, stack);
            }

            plan.AddStep(new CraftingPlanStep(recipe, craftCount, isFinal));
            stack.Remove(recipe);
        }

        private void ResolveItem(
            ItemDefinition item,
            int requiredQuantity,
            Dictionary<ItemDefinition, int> available,
            CraftingPlan plan,
            List<CraftingRecipe> stack)
        {
            if (item == null || requiredQuantity <= 0)
            {
                return;
            }

            plan.AddRequirement(item, requiredQuantity);

            int availableQuantity;
            available.TryGetValue(item, out availableQuantity);
            int usedFromInventory = Mathf.Min(availableQuantity, requiredQuantity);
            if (usedFromInventory > 0)
            {
                available[item] = availableQuantity - usedFromInventory;
                plan.AddReservation(item, usedFromInventory);
            }

            int missingQuantity = requiredQuantity - usedFromInventory;
            if (missingQuantity <= 0)
            {
                return;
            }

            CraftingRecipe producer = FindRecipeFor(item);
            if (producer == null)
            {
                plan.AddMissingMaterial(item, missingQuantity);
                return;
            }

            int craftCount = Mathf.CeilToInt((float)missingQuantity / producer.OutputQuantity);
            ResolveRecipe(producer, craftCount, false, available, plan, stack);

            int producedQuantity = producer.OutputQuantity * craftCount;
            int leftoverQuantity = producedQuantity - missingQuantity;
            if (leftoverQuantity > 0)
            {
                int existing;
                available.TryGetValue(item, out existing);
                available[item] = existing + leftoverQuantity;
            }
        }

        private void CheckStation(CraftingRecipe recipe, CraftingPlan plan)
        {
            if (recipe == null || recipe.StationType == CraftingStationType.Inventory)
            {
                return;
            }

            CraftingPortDefinition port = inventoryCrafting != null ? inventoryCrafting.GetPort(recipe.StationType) : null;
            if (port == null)
            {
                plan.AddBlockingMessage("Missing Crafting Port: equip " + recipe.StationType + " for " + recipe.DisplayName + ".");
                return;
            }

            if (port.PortLevel < recipe.RequiredPortLevel)
            {
                plan.AddBlockingMessage("Insufficient Crafting Port Tier: " + port.DisplayName + " level " + port.PortLevel + " / " + recipe.RequiredPortLevel + ".");
            }

            if (port.AllowedRecipes.Count > 0 && !ContainsRecipe(port, recipe))
            {
                plan.AddBlockingMessage("Recipe Locked: " + recipe.DisplayName + " is not allowed by " + port.DisplayName + ".");
            }
        }

        private Dictionary<ItemDefinition, int> SnapshotInventory()
        {
            Dictionary<ItemDefinition, int> snapshot = new Dictionary<ItemDefinition, int>();
            if (inventory == null)
            {
                return snapshot;
            }

            List<InventoryStack> stacks = inventory.GetSnapshot();
            for (int i = 0; i < stacks.Count; i++)
            {
                InventoryStack stack = stacks[i];
                if (stack == null || stack.Item == null)
                {
                    continue;
                }

                int existing;
                snapshot.TryGetValue(stack.Item, out existing);
                snapshot[stack.Item] = existing + stack.Quantity;
            }

            return snapshot;
        }

        private static bool ContainsRecipe(CraftingPortDefinition port, CraftingRecipe recipe)
        {
            for (int i = 0; i < port.AllowedRecipes.Count; i++)
            {
                if (port.AllowedRecipes[i] == recipe)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
