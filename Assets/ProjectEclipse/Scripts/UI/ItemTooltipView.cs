using ProjectEclipse.Crafting;
using ProjectEclipse.Equipment;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public static class ItemTooltipView
    {
        public static void Draw(ItemDefinition item, int quantity)
        {
            Rect rect = new Rect(Event.current.mousePosition.x + 18f, Event.current.mousePosition.y + 18f, 330f, 250f);
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.BeginHorizontal();
            Texture icon = ItemSlotView.GetIconTexture(item);
            Rect iconRect = GUILayoutUtility.GetRect(44f, 44f, GUILayout.Width(44f), GUILayout.Height(44f));
            if (icon != null)
            {
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUI.Label(iconRect, "MISSING\nICON");
            }
            GUILayout.BeginVertical();
            GUILayout.Label(item.DisplayName);
            GUILayout.Label("Type: " + item.Category);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (quantity > 1)
            {
                GUILayout.Label("Stack: " + quantity + " / " + item.StackLimit);
            }
            if (!string.IsNullOrEmpty(item.Description))
            {
                GUILayout.Label(item.Description);
            }
            GUILayout.Label("Tier: " + item.ResourceTier);
            if (!string.IsNullOrEmpty(item.DroppedBy))
            {
                GUILayout.Label("Dropped by: " + item.DroppedBy);
            }
            if (!string.IsNullOrEmpty(item.CraftingUsage))
            {
                GUILayout.Label("Used for: " + item.CraftingUsage);
            }

            EquipmentDefinition equipment = item as EquipmentDefinition;
            if (equipment != null)
            {
                GUILayout.Label("Slot: " + equipment.Slot + " / " + equipment.EquipmentType);
                GUILayout.Label("Rarity: " + equipment.Rarity);
                GUILayout.Label("Level: " + equipment.LevelRequirement);
                GUILayout.Label("Stats: ATK " + equipment.Stats.Attack + " / DEF " + equipment.Stats.Defense);
                string classText = equipment.ClassRestriction == null || equipment.ClassRestriction.Unrestricted
                    ? "Any"
                    : equipment.ClassRestriction.RequiredClass.ToString();
                GUILayout.Label("Class: " + classText);
                GUILayout.Label("Visual layer: " + equipment.VisualLayer);
                if (!string.IsNullOrEmpty(equipment.SpecialEffectsPlaceholder))
                {
                    GUILayout.Label("Effect: " + equipment.SpecialEffectsPlaceholder);
                }
            }

            ConsumableDefinition consumable = item as ConsumableDefinition;
            if (consumable != null)
            {
                GUILayout.Label("Effect: " + consumable.EffectDescription);
                GUILayout.Label("Duration: " + consumable.DurationSeconds + "s");
                GUILayout.Label("Cooldown: " + consumable.CooldownSeconds + "s");
            }

            CraftingPortDefinition port = item as CraftingPortDefinition;
            if (port != null)
            {
                GUILayout.Label("Unlocks: " + port.StationType);
                GUILayout.Label("Port slot: " + port.PortSlot);
                GUILayout.Label("Speed: x" + port.SpeedMultiplier);
                GUILayout.Label("Recipes: " + port.AllowedRecipes.Count);
                if (!string.IsNullOrEmpty(port.FuelRules))
                {
                    GUILayout.Label("Fuel: " + port.FuelRules);
                }
            }
            GUILayout.EndArea();
        }
    }
}
