using ProjectEclipse.Crafting;
using ProjectEclipse.Equipment;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public enum ItemSlotClick
    {
        None,
        Left,
        Right
    }

    public static class ItemSlotView
    {
        private const float SlotSize = 46f;

        public static bool Draw(ItemDefinition item, int quantity, ItemHoverState hover, bool selected = false)
        {
            return DrawClick(item, quantity, hover, selected) != ItemSlotClick.None;
        }

        public static ItemSlotClick DrawClick(ItemDefinition item, int quantity, ItemHoverState hover, bool selected = false)
        {
            Rect rect = GUILayoutUtility.GetRect(SlotSize, SlotSize, GUILayout.Width(SlotSize), GUILayout.Height(SlotSize));
            DrawFrame(rect, selected);

            if (item != null)
            {
                DrawItemContents(rect, item, quantity);

                if (rect.Contains(Event.current.mousePosition))
                {
                    hover.SetItem(item, quantity);
                }
            }

            return ReadClick(rect);
        }

        public static ItemSlotClick DrawEquipmentSlot(ItemDefinition item, int quantity, ItemHoverState hover, EquipmentSlot slot, string label, bool selected = false)
        {
            Rect rect = GUILayoutUtility.GetRect(SlotSize, SlotSize, GUILayout.Width(SlotSize), GUILayout.Height(SlotSize));
            DrawFrame(rect, selected);

            if (item != null)
            {
                DrawItemContents(rect, item, quantity);
            }

            if (rect.Contains(Event.current.mousePosition))
            {
                if (item != null)
                {
                    hover.SetItem(item, quantity);
                }
                else
                {
                    hover.SetEquipmentSlot(slot, label, "Empty " + label + " slot. Right-click inventory gear that matches this slot to equip it.");
                }
            }

            return ReadClick(rect);
        }

        public static ItemSlotClick DrawCraftingPortSlot(ItemDefinition item, int quantity, ItemHoverState hover, CraftingPortSlot slot, string label, bool selected = false)
        {
            Rect rect = GUILayoutUtility.GetRect(SlotSize, SlotSize, GUILayout.Width(SlotSize), GUILayout.Height(SlotSize));
            DrawFrame(rect, selected);

            if (item != null)
            {
                DrawItemContents(rect, item, quantity);
            }

            if (rect.Contains(Event.current.mousePosition))
            {
                if (item != null)
                {
                    hover.SetItem(item, quantity);
                }
                else
                {
                    hover.SetCraftingPortSlot(slot, label, "Empty " + label + ". Right-click a matching crafting trinket in inventory to socket it.");
                }
            }

            return ReadClick(rect);
        }

        public static Texture GetIconTexture(ItemDefinition item)
        {
            return item != null && item.Icon != null ? item.Icon.texture : null;
        }

        private static void DrawFrame(Rect rect, bool selected)
        {
            Color oldColor = GUI.color;
            GUI.color = selected ? new Color(1f, 0.94f, 0.62f, 1f) : Color.white;
            GUI.Box(rect, string.Empty);
            GUI.color = oldColor;
        }

        private static void DrawItemContents(Rect rect, ItemDefinition item, int quantity)
        {
            Rect iconRect = new Rect(rect.x + 5f, rect.y + 5f, 32f, 32f);
            Texture icon = GetIconTexture(item);
            if (icon != null)
            {
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
            }
            else
            {
                Color oldColor = GUI.color;
                GUI.color = Color.magenta;
                GUI.Box(iconRect, "!");
                GUI.color = oldColor;
                GUI.Label(new Rect(rect.x + 3f, rect.y + 28f, 43f, 16f), "NO ICON");
            }

            if (quantity > 1)
            {
                GUI.Label(new Rect(rect.x + 4f, rect.y + 29f, 40f, 16f), quantity.ToString());
            }

            EquipmentDefinition equipment = item as EquipmentDefinition;
            if (equipment != null)
            {
                GUI.Label(new Rect(rect.x + 31f, rect.y + 1f, 14f, 16f), RarityMarker(equipment.Rarity));
            }
            else
            {
                GUI.Label(new Rect(rect.x + 31f, rect.y + 1f, 14f, 16f), TierMarker(item));
            }
        }

        private static ItemSlotClick ReadClick(Rect rect)
        {
            Event current = Event.current;
            if (current == null || current.type != EventType.MouseDown || !rect.Contains(current.mousePosition))
            {
                return ItemSlotClick.None;
            }

            if (current.button == 0)
            {
                current.Use();
                return ItemSlotClick.Left;
            }

            if (current.button == 1)
            {
                current.Use();
                return ItemSlotClick.Right;
            }

            return ItemSlotClick.None;
        }

        private static string TierMarker(ItemDefinition item)
        {
            return item.ResourceTier.ToString().Substring(0, 1);
        }

        private static string RarityMarker(EquipmentRarity rarity)
        {
            return rarity.ToString().Substring(0, 1);
        }
    }
}
