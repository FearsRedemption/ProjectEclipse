using System.Collections.Generic;
using ProjectEclipse.Crafting;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public class CraftingPanel
    {
        private enum CraftingFilter
        {
            All,
            Weapons,
            Armor,
            CraftingPorts,
            Materials,
            Consumables,
            Upgrades
        }

        private readonly CraftingSystem crafting;
        private Vector2 scroll;
        private CraftingFilter selectedFilter = CraftingFilter.All;
        private bool availableOnly;
        private string search = string.Empty;

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

            DrawIntegrated(220f);
            GUI.DragWindow();
        }

        public void DrawIntegrated(float height)
        {
            if (crafting == null)
            {
                GUILayout.Label("Crafting missing.");
                return;
            }

            DrawFilters();
            search = GUILayout.TextField(search);
            availableOnly = GUILayout.Toggle(availableOnly, "Available Only");

            IReadOnlyList<CraftingRecipe> recipes = crafting.Recipes;
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(height));
            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe recipe = recipes[i];
                if (!ShouldShow(recipe))
                {
                    continue;
                }

                GUI.enabled = crafting.CanCraft(recipe);
                if (GUILayout.Button(recipe.DisplayName))
                {
                    crafting.TryCraft(recipe);
                }
                GUI.enabled = true;
                GUILayout.Label(DescribeRecipe(recipe));
                string missing = DescribeMissing(recipe);
                if (!string.IsNullOrEmpty(missing))
                {
                    GUILayout.Label(missing);
                }
                GUILayout.Space(5f);
            }
            GUILayout.EndScrollView();
        }

        private void DrawFilters()
        {
            GUILayout.BeginHorizontal();
            FilterButton(CraftingFilter.All, "All");
            FilterButton(CraftingFilter.Weapons, "Weapons");
            FilterButton(CraftingFilter.Armor, "Armor");
            FilterButton(CraftingFilter.CraftingPorts, "Ports");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            FilterButton(CraftingFilter.Materials, "Materials");
            FilterButton(CraftingFilter.Consumables, "Consumables");
            FilterButton(CraftingFilter.Upgrades, "Upgrades");
            GUILayout.EndHorizontal();
        }

        private void FilterButton(CraftingFilter filter, string label)
        {
            GUI.enabled = selectedFilter != filter;
            if (GUILayout.Button(label, GUILayout.Height(24f)))
            {
                selectedFilter = filter;
            }
            GUI.enabled = true;
        }

        private bool ShouldShow(CraftingRecipe recipe)
        {
            if (recipe == null || recipe.OutputItem == null)
            {
                return false;
            }

            if (availableOnly && !crafting.CanCraft(recipe))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(search))
            {
                string haystack = recipe.DisplayName + " " + recipe.OutputItem.DisplayName + " " + recipe.StationType;
                if (haystack.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }

            ItemCategory category = recipe.OutputItem.Category;
            switch (selectedFilter)
            {
                case CraftingFilter.Weapons:
                    return category == ItemCategory.Weapon;
                case CraftingFilter.Armor:
                    return category == ItemCategory.Armor;
                case CraftingFilter.CraftingPorts:
                    return category == ItemCategory.CraftingPort;
                case CraftingFilter.Materials:
                    return category == ItemCategory.Material;
                case CraftingFilter.Consumables:
                    return category == ItemCategory.Consumable;
                case CraftingFilter.Upgrades:
                    return category == ItemCategory.Upgrade;
                default:
                    return true;
            }
        }

        private string DescribeRecipe(CraftingRecipe recipe)
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
                    int count = crafting.CountItem(ingredient.Item);
                    parts.Add(ingredient.Item.DisplayName + " " + count + "/" + ingredient.Quantity);
                }
            }

            string output = recipe.OutputItem != null ? recipe.OutputItem.DisplayName + " x" + recipe.OutputQuantity : "Unknown";
            return "[" + recipe.StationType + "] " + string.Join(", ", parts.ToArray()) + " -> " + output;
        }

        private string DescribeMissing(CraftingRecipe recipe)
        {
            if (recipe == null)
            {
                return string.Empty;
            }

            List<string> missing = new List<string>();
            if (!crafting.HasRequiredStation(recipe))
            {
                missing.Add("Equip " + recipe.StationType);
            }

            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = recipe.Ingredients[i];
                if (ingredient == null || ingredient.Item == null)
                {
                    continue;
                }

                int count = crafting.CountItem(ingredient.Item);
                if (count < ingredient.Quantity)
                {
                    missing.Add(ingredient.Item.DisplayName + " +" + (ingredient.Quantity - count));
                }
            }

            return missing.Count > 0 ? "Missing: " + string.Join(", ", missing.ToArray()) : string.Empty;
        }
    }
}
