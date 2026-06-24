using ProjectEclipse.Crafting;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public static class CraftingPortComparisonTooltipView
    {
        public static void Draw(CraftingPortDefinition hovered, InventoryCraftingController inventoryCrafting)
        {
            if (hovered == null)
            {
                return;
            }

            CraftingPortDefinition equipped = inventoryCrafting != null ? inventoryCrafting.GetEquippedPort(hovered.PortSlot) : null;
            GUILayout.Label("Trinket slot: " + CraftingTerminology.GetSlotDisplayName(hovered.PortSlot));
            GUILayout.Label("Station: " + CraftingTerminology.GetStationDisplayName(hovered.StationType));
            GUILayout.Label("Tier: " + CraftingTrinketTierUtility.FormatTier(hovered.PortLevel) + Delta(hovered.PortLevel, equipped != null ? equipped.PortLevel : 0));
            GUILayout.Label("Speed: x" + hovered.SpeedMultiplier.ToString("0.##") + Delta(hovered.SpeedMultiplier, equipped != null ? equipped.SpeedMultiplier : 1f));
            GUILayout.Label("Fuel efficiency: x" + hovered.FuelEfficiency.ToString("0.##") + Delta(hovered.FuelEfficiency, equipped != null ? equipped.FuelEfficiency : 1f));
            GUILayout.Label("Recipes: " + hovered.AllowedRecipes.Count);
            if (!string.IsNullOrEmpty(hovered.AllowedRecipeCategories))
            {
                GUILayout.Label("Categories: " + hovered.AllowedRecipeCategories);
            }
            if (!string.IsNullOrEmpty(hovered.FuelRules))
            {
                GUILayout.Label("Fuel: " + hovered.FuelRules);
            }
            if (!string.IsNullOrEmpty(hovered.SpecialEffectText))
            {
                GUILayout.Label("Effect: " + hovered.SpecialEffectText);
            }
            if (!string.IsNullOrEmpty(hovered.UpgradeRequirements))
            {
                GUILayout.Label("Upgrade: " + hovered.UpgradeRequirements);
            }
            GUILayout.Label("Compared to: " + (equipped != null ? equipped.DisplayName : "Empty"));
        }

        private static string Delta(int hovered, int equipped)
        {
            int delta = hovered - equipped;
            return delta == 0 ? string.Empty : " (" + (delta > 0 ? "+" : string.Empty) + delta + ")";
        }

        private static string Delta(float hovered, float equipped)
        {
            float delta = hovered - equipped;
            return Mathf.Abs(delta) < 0.001f ? string.Empty : " (" + (delta > 0f ? "+" : string.Empty) + delta.ToString("0.##") + ")";
        }
    }
}
