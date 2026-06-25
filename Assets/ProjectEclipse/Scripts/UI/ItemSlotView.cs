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
        public const float SlotSize = 48f;
        public const float IconSize = 34f;

        public static bool Draw(ItemDefinition item, int quantity, ItemHoverState hover, bool selected = false)
        {
            return DrawClick(item, quantity, hover, selected) != ItemSlotClick.None;
        }

        public static ItemSlotClick DrawClick(ItemDefinition item, int quantity, ItemHoverState hover, bool selected = false)
        {
            Rect rect = GUILayoutUtility.GetRect(SlotSize, SlotSize, GUILayout.Width(SlotSize), GUILayout.Height(SlotSize));
            return DrawClick(rect, item, quantity, hover, selected);
        }

        public static ItemSlotClick DrawClick(Rect rect, ItemDefinition item, int quantity, ItemHoverState hover, bool selected = false)
        {
            DrawFrame(rect, selected);
            if (item != null)
            {
                DrawItemContents(rect, item, quantity);

                if (hover != null && rect.Contains(Event.current.mousePosition))
                {
                    hover.SetItem(item, quantity);
                }
            }

            return ReadClick(rect);
        }

        public static void DrawEmpty()
        {
            Rect rect = GUILayoutUtility.GetRect(SlotSize, SlotSize, GUILayout.Width(SlotSize), GUILayout.Height(SlotSize));
            DrawEmpty(rect);
        }

        public static void DrawEmpty(Rect rect)
        {
            DrawFrame(rect, false);
            Rect inner = new Rect(rect.x + 12f, rect.y + 12f, rect.width - 24f, rect.height - 24f);
            GameGuiStyles.DrawBox(inner, new Color(0.1f, 0.12f, 0.12f, 0.65f), new Color(0.25f, 0.3f, 0.29f, 0.7f), 1f);
        }

        public static ItemSlotClick DrawEquipmentSlot(ItemDefinition item, int quantity, ItemHoverState hover, EquipmentSlot slot, string label, bool selected = false)
        {
            Rect rect = GUILayoutUtility.GetRect(SlotSize, SlotSize, GUILayout.Width(SlotSize), GUILayout.Height(SlotSize));
            return DrawEquipmentSlot(rect, item, quantity, hover, slot, label, selected);
        }

        public static ItemSlotClick DrawEquipmentSlot(Rect rect, ItemDefinition item, int quantity, ItemHoverState hover, EquipmentSlot slot, string label, bool selected = false)
        {
            DrawFrame(rect, selected);

            if (item != null)
            {
                DrawItemContents(rect, item, quantity);
            }
            else
            {
                DrawEmptyGlyph(rect);
            }

            if (hover != null && rect.Contains(Event.current.mousePosition))
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
            return DrawCraftingPortSlot(rect, item, quantity, hover, slot, label, selected);
        }

        public static ItemSlotClick DrawCraftingPortSlot(Rect rect, ItemDefinition item, int quantity, ItemHoverState hover, CraftingPortSlot slot, string label, bool selected = false)
        {
            DrawFrame(rect, selected);

            if (item != null)
            {
                DrawItemContents(rect, item, quantity);
            }
            else
            {
                DrawEmptyGlyph(rect);
            }

            if (hover != null && rect.Contains(Event.current.mousePosition))
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
            GameGuiStyles.ApplySkin(GUI.skin);
            GameGuiStyles.DrawSlot(rect, selected);
        }

        private static void DrawItemContents(Rect rect, ItemDefinition item, int quantity)
        {
            Rect iconRect = new Rect(rect.x + 7f, rect.y + 6f, IconSize, IconSize);
            Texture icon = GetIconTexture(item);
            if (icon != null)
            {
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
            }
            else
            {
                DrawGeneratedIcon(iconRect, item);
            }

            if (quantity > 1)
            {
                Rect countBack = new Rect(rect.x + rect.width - 25f, rect.y + rect.height - 16f, 22f, 13f);
                GUI.DrawTexture(countBack, GameGuiStyles.GetTexture(new Color(0f, 0f, 0f, 0.55f)));
                GUI.Label(countBack, quantity.ToString(), GameGuiStyles.StackLabel);
            }

            EquipmentDefinition equipment = item as EquipmentDefinition;
            string badge = equipment != null ? RarityMarker(equipment.Rarity) : TierMarker(item);
            Rect badgeRect = new Rect(rect.x + rect.width - 16f, rect.y + 2f, 13f, 13f);
            GUI.Label(badgeRect, badge, GameGuiStyles.BadgeLabel);
        }

        private static void DrawGeneratedIcon(Rect rect, ItemDefinition item)
        {
            Color tint = item != null ? item.PlaceholderColor : Color.gray;
            GameGuiStyles.DrawBox(rect, Color.Lerp(tint, Color.black, 0.25f), Color.Lerp(tint, Color.white, 0.45f), 1f);
            string letter = "?";
            if (item != null && !string.IsNullOrEmpty(item.DisplayName))
            {
                letter = item.DisplayName.Substring(0, 1).ToUpperInvariant();
            }

            GUI.Label(rect, letter, GameGuiStyles.CenterLabel);
        }

        private static void DrawEmptyGlyph(Rect rect)
        {
            Rect glyph = new Rect(rect.x + 16f, rect.y + 16f, rect.width - 32f, rect.height - 32f);
            GameGuiStyles.DrawBox(glyph, new Color(0.09f, 0.11f, 0.11f, 0.8f), new Color(0.28f, 0.34f, 0.33f, 0.85f), 1f);
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
