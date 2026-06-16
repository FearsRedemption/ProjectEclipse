using System.Collections.Generic;
using ProjectEclipse.Crafting;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public class CraftingPanel
    {
        private readonly CraftingSystem crafting;

        public CraftingPanel(CraftingSystem crafting)
        {
            this.crafting = crafting;
        }

        public void Draw(int windowId)
        {
            if (crafting == null)
            {
                GUILayout.Label("Crafting missing.");
                return;
            }

            IReadOnlyList<CraftingRecipe> recipes = crafting.Recipes;
            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe recipe = recipes[i];
                GUI.enabled = crafting.CanCraft(recipe);
                if (GUILayout.Button(recipe.DisplayName))
                {
                    crafting.TryCraft(recipe);
                }
                GUI.enabled = true;
                GUILayout.Label(DescribeRecipe(recipe));
            }

            GUI.DragWindow();
        }

        private static string DescribeRecipe(CraftingRecipe recipe)
        {
            if (recipe == null)
            {
                return string.Empty;
            }

            List<string> parts = new List<string>();
            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = recipe.Ingredients[i];
                if (ingredient != null && ingredient.Item != null)
                {
                    parts.Add(ingredient.Item.DisplayName + " x" + ingredient.Quantity);
                }
            }

            string output = recipe.OutputItem != null ? recipe.OutputItem.DisplayName + " x" + recipe.OutputQuantity : "Unknown";
            return "[" + recipe.StationType + "] " + string.Join(", ", parts.ToArray()) + " -> " + output;
        }
    }
}
