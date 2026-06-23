using ProjectEclipse.Equipment;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public static class EquipmentComparisonTooltipView
    {
        public static void Draw(EquipmentDefinition hovered, EquipmentController equipment)
        {
            if (hovered == null)
            {
                return;
            }

            EquipmentDefinition equipped = equipment != null ? equipment.GetEquippedItem(hovered.Slot) as EquipmentDefinition : null;
            GUILayout.Label("Slot: " + hovered.Slot + " / " + hovered.EquipmentType);
            GUILayout.Label("Rarity: " + hovered.Rarity);
            GUILayout.Label("Level: " + hovered.LevelRequirement);
            string classText = hovered.ClassRestriction == null || hovered.ClassRestriction.Unrestricted
                ? "Any"
                : hovered.ClassRestriction.RequiredClass.ToString();
            GUILayout.Label("Class: " + classText);
            GUILayout.Label("Visual layer: " + hovered.VisualLayer);
            if (!string.IsNullOrEmpty(hovered.SpecialEffectsPlaceholder))
            {
                GUILayout.Label("Effect: " + hovered.SpecialEffectsPlaceholder);
            }

            GUILayout.Space(4f);
            GUILayout.Label("Compared to: " + (equipped != null ? equipped.DisplayName : "Empty"));
            DrawStatDelta("Attack", hovered.Stats.Attack, equipped != null ? equipped.Stats.Attack : 0);
            DrawStatDelta("Defense", hovered.Stats.Defense, equipped != null ? equipped.Stats.Defense : 0);
            DrawStatDelta("Health", hovered.Stats.MaxHealth, equipped != null ? equipped.Stats.MaxHealth : 0);
            DrawStatDelta("Move", hovered.Stats.MoveSpeedBonus, equipped != null ? equipped.Stats.MoveSpeedBonus : 0f);
            DrawStatDelta("Jump", hovered.Stats.JumpForceBonus, equipped != null ? equipped.Stats.JumpForceBonus : 0f);
            DrawStatDelta("Air", hovered.Stats.AirControlBonus, equipped != null ? equipped.Stats.AirControlBonus : 0f);
        }

        private static void DrawStatDelta(string label, int hovered, int equipped)
        {
            int delta = hovered - equipped;
            GUILayout.Label(label + ": " + hovered + " (" + FormatDelta(delta) + ")");
        }

        private static void DrawStatDelta(string label, float hovered, float equipped)
        {
            float delta = hovered - equipped;
            GUILayout.Label(label + ": " + hovered.ToString("0.##") + " (" + FormatDelta(delta) + ")");
        }

        private static string FormatDelta(int value)
        {
            return value >= 0 ? "+" + value : value.ToString();
        }

        private static string FormatDelta(float value)
        {
            return value >= 0f ? "+" + value.ToString("0.##") : value.ToString("0.##");
        }
    }
}
