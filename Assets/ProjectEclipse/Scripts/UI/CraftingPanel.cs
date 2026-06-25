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
        private Vector2 detailsScroll;

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
            GameGuiStyles.ApplySkin(GUI.skin);
            if (crafting == null)
            {
                GUILayout.Label("Crafting missing.");
                return;
            }

            DrawFilters();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search", GameGuiStyles.SmallLabel, GUILayout.Width(48f));
            search = GUILayout.TextField(search, GUILayout.Width(170f));
            availableOnly = GUILayout.Toggle(availableOnly, "Available Only", GUILayout.Width(130f));
            GUILayout.EndHorizontal();

            CraftingFeedbackView.DrawCompact(crafting.Feedback);

            float panelHeight = Mathf.Max(140f, height);
            GUILayout.BeginHorizontal(GUILayout.Height(panelHeight));
            DrawRecipeList(panelHeight);
            GUILayout.Space(6f);
            DrawSelectedRecipeDetails(panelHeight);
            GUILayout.EndHorizontal();
        }

        private void DrawRecipeList(float height)
        {
            GUILayout.BeginVertical(GameGuiStyles.SubPanel, GUILayout.Width(220f), GUILayout.Height(height));
            GUILayout.Label("Recipes", GameGuiStyles.HeaderLabel);
            IReadOnlyList<CraftingRecipe> recipes = crafting.Recipes;
            int visibleCount = 0;
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(Mathf.Max(60f, height - 28f)));
            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe recipe = recipes[i];
                if (!ShouldShow(recipe))
                {
                    continue;
                }

                visibleCount++;
                bool isSelected = recipe == selectedRecipe;
                GUIStyle style = isSelected ? GameGuiStyles.SelectedButton : GameGuiStyles.Button;
                if (GUILayout.Button(recipe.DisplayName, style, GUILayout.Height(26f)))
                {
                    SelectRecipe(recipe);
                }
            }

            if (visibleCount == 0)
            {
                GUILayout.Label("No matching recipes", GameGuiStyles.MutedLabel);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawSelectedRecipeDetails(float height)
        {
            GUILayout.BeginVertical(GameGuiStyles.SubPanel, GUILayout.Width(264f), GUILayout.Height(height));
            if (selectedRecipe == null)
            {
                GUILayout.Label("Select a recipe", GameGuiStyles.HeaderLabel);
                GUILayout.Label("Recipe details and requirements appear here.", GameGuiStyles.MutedLabel);
                GUILayout.EndVertical();
                return;
            }

            GUILayout.Label(selectedRecipe.DisplayName, GameGuiStyles.HeaderLabel);
            string output = selectedRecipe.OutputItem != null ? selectedRecipe.OutputItem.DisplayName + " x" + selectedRecipe.OutputQuantity : "Unknown Output";
            GUILayout.Label("Output: " + output, GameGuiStyles.SmallLabel);
            GUILayout.Label("Station: " + CraftingTerminology.GetStationDisplayName(selectedRecipe.StationType), GameGuiStyles.SmallLabel);
            DrawAmountSelector();

            if (GUILayout.Button("Start Work Order", GameGuiStyles.Button, GUILayout.Height(28f)))
            {
                crafting.TryStartWorkOrder(selectedRecipe, GetCraftAmount());
                ResetCraftAmount();
            }

            GUILayout.Label("Requirements", GameGuiStyles.SmallLabel);
            showRequirementDetails = GUILayout.Toggle(showRequirementDetails, "Show dependency details");
            List<CraftingRequirementLine> preview = crafting.GetRecipePreview(selectedRecipe, GetCraftAmount(), showRequirementDetails);
            detailsScroll = GUILayout.BeginScrollView(detailsScroll, GUILayout.Height(Mathf.Max(54f, height - 140f)));
            CraftingFeedbackView.DrawLines(preview);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawFilters()
        {
            GUILayout.BeginHorizontal();
            FilterButton(CraftingFilter.All, "All");
            FilterButton(CraftingFilter.Handcrafted, "Hand");
            FilterButton(CraftingFilter.Weapons, "Weapons");
            FilterButton(CraftingFilter.Armor, "Armor");
            FilterButton(CraftingFilter.CraftingPorts, "Trinkets");
            FilterButton(CraftingFilter.Materials, "Materials");
            FilterButton(CraftingFilter.Consumables, "Usable");
            FilterButton(CraftingFilter.Upgrades, "Upgrades");
            GUILayout.EndHorizontal();
        }

        private void FilterButton(CraftingFilter filter, string label)
        {
            GUIStyle style = selectedFilter == filter ? GameGuiStyles.SelectedButton : GameGuiStyles.Button;
            if (GUILayout.Button(label, style, GUILayout.Height(24f)))
            {
                selectedFilter = filter;
                ResetCraftAmount();
            }
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
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            AmountButton(50);
            AmountButton(100);
            GUI.enabled = !customAmountSelected;
            if (GUILayout.Button("Custom", GUILayout.Width(62f)))
            {
                customAmountSelected = true;
                customAmountText = craftAmount.ToString();
            }
            GUI.enabled = true;
            GUILayout.Label("x", GUILayout.Width(12f));
            string nextCustom = GUILayout.TextField(customAmountText, GUILayout.Width(50f));
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
                missing.Add("Socket " + CraftingTerminology.GetStationDisplayName(recipe.StationType));
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
