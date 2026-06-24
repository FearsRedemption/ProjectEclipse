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
            Draw(item, quantity, null, null);
        }

        public static void Draw(ItemHoverState hover, EquipmentController equipment, InventoryCraftingController inventoryCrafting)
        {
            if (hover == null || !hover.HasHover)
            {
                return;
            }

            if (hover.Item != null)
            {
                Draw(hover.Item, hover.Quantity, equipment, inventoryCrafting);
                return;
            }

            DrawEmptySlot(hover);
        }

        public static void Draw(ItemDefinition item, int quantity, EquipmentController equipment, InventoryCraftingController inventoryCrafting)
        {
            if (item == null)
            {
                return;
            }

            Rect rect = new Rect(Event.current.mousePosition.x + 18f, Event.current.mousePosition.y + 18f, 350f, 360f);
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

            EquipmentDefinition equipmentItem = item as EquipmentDefinition;
            if (equipmentItem != null)
            {
                EquipmentComparisonTooltipView.Draw(equipmentItem, equipment);
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
                CraftingPortComparisonTooltipView.Draw(port, inventoryCrafting);
            }
            GUILayout.EndArea();
        }

        private static void DrawEmptySlot(ItemHoverState hover)
        {
            Rect rect = new Rect(Event.current.mousePosition.x + 18f, Event.current.mousePosition.y + 18f, 300f, 95f);
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label(hover.SlotLabel);
            if (hover.Kind == ItemHoverKind.EquipmentSlot)
            {
                GUILayout.Label("Equipment slot: " + hover.EquipmentSlot);
            }
            else if (hover.Kind == ItemHoverKind.CraftingPortSlot)
            {
                GUILayout.Label("Crafting trinket slot: " + CraftingTerminology.GetSlotDisplayName(hover.CraftingPortSlot));
            }
            if (!string.IsNullOrEmpty(hover.SlotDescription))
            {
                GUILayout.Label(hover.SlotDescription);
            }
            GUILayout.EndArea();
        }
    }
}
