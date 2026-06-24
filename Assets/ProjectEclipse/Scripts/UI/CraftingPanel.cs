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
            Handcrafted,
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
        private CraftingRecipe selectedRecipe;
        private int craftAmount = 1;
        private bool customAmountSelected;
        private string customAmountText = "1";
        private bool showRequirementDetails;

        private const int MaxCraftAmount = 9999;

        public CraftingPanel(CraftingSystem crafting)
        {
            this.crafting = crafting;
        }

        public void ResetCraftAmount()
        {
            craftAmount = 1;
            customAmountText = "1";
            customAmountSelected = false;
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

            CraftingFeedbackView.Draw(crafting.Feedback);
            DrawSelectedRecipeControls();
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

                bool isSelected = recipe == selectedRecipe;
                Color oldColor = GUI.color;
                GUI.color = isSelected ? new Color(1f, 0.94f, 0.62f, 1f) : Color.white;
                if (GUILayout.Button(recipe.DisplayName))
                {
                    SelectRecipe(recipe);
                }
                GUI.color = oldColor;
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
            FilterButton(CraftingFilter.Handcrafted, "Handcrafted");
            FilterButton(CraftingFilter.Weapons, "Weapons");
            FilterButton(CraftingFilter.Armor, "Armor");
            FilterButton(CraftingFilter.CraftingPorts, "Trinkets");
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
                ResetCraftAmount();
            }
            GUI.enabled = true;
        }

        private void DrawSelectedRecipeControls()
        {
            if (selectedRecipe == null)
            {
                GUILayout.Label("Select a recipe to create a Work Order.");
                return;
            }

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(selectedRecipe.DisplayName);
            DrawAmountSelector();
            if (GUILayout.Button("CRAFT", GUILayout.Height(28f)))
            {
                crafting.TryStartWorkOrder(selectedRecipe, GetCraftAmount());
                ResetCraftAmount();
            }

            GUILayout.Label("Requirements");
            showRequirementDetails = GUILayout.Toggle(showRequirementDetails, "Show Details");
            List<CraftingRequirementLine> preview = crafting.GetRecipePreview(selectedRecipe, GetCraftAmount(), showRequirementDetails);
            CraftingFeedbackView.DrawLines(preview);
            GUILayout.EndVertical();
        }

        private void DrawAmountSelector()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Craft:", GUILayout.Width(42f));
            AmountButton(1);
            AmountButton(5);
            AmountButton(10);
            AmountButton(50);
            AmountButton(100);
            GUI.enabled = !customAmountSelected;
            if (GUILayout.Button("Custom", GUILayout.Width(72f)))
            {
                customAmountSelected = true;
                customAmountText = craftAmount.ToString();
            }
            GUI.enabled = true;
            GUILayout.Label("x", GUILayout.Width(12f));
            string nextCustom = GUILayout.TextField(customAmountText, GUILayout.Width(72f));
            if (nextCustom != customAmountText)
            {
                customAmountSelected = true;
                customAmountText = SanitizeAmountText(nextCustom);
                craftAmount = ParseCustomAmount(customAmountText);
            }
            GUILayout.EndHorizontal();
        }

        private void AmountButton(int amount)
        {
            GUI.enabled = craftAmount != amount || customAmountSelected;
            if (GUILayout.Button(amount.ToString(), GUILayout.Width(42f)))
            {
                craftAmount = Mathf.Clamp(amount, 1, MaxCraftAmount);
                customAmountText = craftAmount.ToString();
                customAmountSelected = false;
            }
            GUI.enabled = true;
        }

        private void SelectRecipe(CraftingRecipe recipe)
        {
            if (selectedRecipe != recipe)
            {
                selectedRecipe = recipe;
                showRequirementDetails = false;
                ResetCraftAmount();
            }
        }

        private int GetCraftAmount()
        {
            if (customAmountSelected)
            {
                craftAmount = ParseCustomAmount(customAmountText);
                customAmountText = craftAmount.ToString();
            }

            return Mathf.Clamp(craftAmount, 1, MaxCraftAmount);
        }

        private static string SanitizeAmountText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            List<char> digits = new List<char>();
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsDigit(value[i]))
                {
                    digits.Add(value[i]);
                }
            }

            return new string(digits.ToArray());
        }

        private static int ParseCustomAmount(string value)
        {
            int parsed;
            if (!int.TryParse(value, out parsed))
            {
                return 1;
            }

            return Mathf.Clamp(parsed, 1, MaxCraftAmount);
        }

        private bool ShouldShow(CraftingRecipe recipe)
        {
            if (recipe == null || recipe.OutputItem == null)
            {
                return false;
            }

            if (availableOnly && !crafting.CanQueueWorkOrder(recipe, GetCraftAmount()))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(search))
            {
                string haystack = recipe.DisplayName + " " + recipe.OutputItem.DisplayName + " " + CraftingTerminology.GetStationDisplayName(recipe.StationType);
                if (haystack.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }

            ItemCategory category = recipe.OutputItem.Category;
            switch (selectedFilter)
            {
                case CraftingFilter.Handcrafted:
                    return recipe.StationType == CraftingStationType.Inventory;
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
                    int available = crafting.CountAvailableItem(ingredient.Item);
                    int reserved = crafting.CountReservedItem(ingredient.Item);
                    string reservedText = reserved > 0 ? " (" + reserved + " reserved)" : string.Empty;
                    parts.Add(ingredient.Item.DisplayName + " " + available + "/" + ingredient.Quantity + reservedText);
                }
            }

            string output = recipe.OutputItem != null ? recipe.OutputItem.DisplayName + " x" + recipe.OutputQuantity : "Unknown";
            return "[" + CraftingTerminology.GetStationDisplayName(recipe.StationType) + "] " + string.Join(", ", parts.ToArray()) + " -> " + output;
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
                missing.Add("Equip " + CraftingTerminology.GetStationDisplayName(recipe.StationType));
            }

            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = recipe.Ingredients[i];
                if (ingredient == null || ingredient.Item == null)
                {
                    continue;
                }

                int available = crafting.CountAvailableItem(ingredient.Item);
                if (available < ingredient.Quantity)
                {
                    missing.Add(ingredient.Item.DisplayName + " +" + (ingredient.Quantity - available));
                }
            }

            return missing.Count > 0 ? "Missing: " + string.Join(", ", missing.ToArray()) : string.Empty;
        }
    }
}
