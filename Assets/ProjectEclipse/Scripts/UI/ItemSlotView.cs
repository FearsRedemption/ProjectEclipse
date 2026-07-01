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
            Color dark = Color.Lerp(tint, Color.black, 0.46f);
            Color light = Color.Lerp(tint, Color.white, 0.42f);
            GameGuiStyles.DrawBox(rect, dark, light, 1f);

            Rect shine = new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, 3f);
            GUI.DrawTexture(shine, GameGuiStyles.GetTexture(new Color(1f, 1f, 1f, 0.16f)));

            if (item == null)
            {
                DrawUnknownGlyph(rect, tint);
                return;
            }

            switch (item.Category)
            {
                case ItemCategory.Weapon:
                    DrawWeaponGlyph(rect, tint);
                    break;
                case ItemCategory.Armor:
                    DrawArmorGlyph(rect, tint);
                    break;
                case ItemCategory.CraftingPort:
                case ItemCategory.Upgrade:
                    DrawTrinketGlyph(rect, tint);
                    break;
                case ItemCategory.KeyItem:
                case ItemCategory.Quest:
                    DrawKeyGlyph(rect, tint);
                    break;
                case ItemCategory.Consumable:
                    DrawBottleGlyph(rect, tint);
                    break;
                default:
                    DrawMaterialGlyph(rect, tint);
                    break;
            }
        }

        private static void DrawMaterialGlyph(Rect rect, Color tint)
        {
            Color fill = Color.Lerp(tint, Color.white, 0.18f);
            Color shade = Color.Lerp(tint, Color.black, 0.35f);
            DrawDiamond(rect, 0.5f, 0.54f, 0.34f, fill, shade);
            DrawMiniRect(rect, 0.36f, 0.28f, 0.18f, 0.12f, Color.Lerp(tint, Color.white, 0.55f));
            DrawMiniRect(rect, 0.56f, 0.64f, 0.16f, 0.1f, shade);
        }

        private static void DrawWeaponGlyph(Rect rect, Color tint)
        {
            Color metal = Color.Lerp(tint, Color.white, 0.35f);
            Color shade = Color.Lerp(tint, Color.black, 0.34f);
            DrawMiniRect(rect, 0.36f, 0.68f, 0.34f, 0.09f, shade);
            DrawMiniRect(rect, 0.45f, 0.28f, 0.12f, 0.5f, metal);
            DrawMiniRect(rect, 0.53f, 0.32f, 0.06f, 0.42f, Color.Lerp(metal, Color.white, 0.38f));
            DrawMiniRect(rect, 0.39f, 0.76f, 0.24f, 0.09f, new Color(0.28f, 0.17f, 0.1f, 1f));
        }

        private static void DrawArmorGlyph(Rect rect, Color tint)
        {
            Color fill = Color.Lerp(tint, Color.white, 0.2f);
            Color shade = Color.Lerp(tint, Color.black, 0.32f);
            DrawMiniRect(rect, 0.32f, 0.27f, 0.36f, 0.18f, fill);
            DrawMiniRect(rect, 0.27f, 0.38f, 0.46f, 0.34f, fill);
            DrawMiniRect(rect, 0.34f, 0.7f, 0.32f, 0.12f, shade);
            DrawMiniRect(rect, 0.47f, 0.4f, 0.06f, 0.36f, shade);
        }

        private static void DrawTrinketGlyph(Rect rect, Color tint)
        {
            Color fill = Color.Lerp(tint, Color.white, 0.28f);
            Color shade = Color.Lerp(tint, Color.black, 0.4f);
            DrawMiniRect(rect, 0.37f, 0.2f, 0.26f, 0.14f, shade);
            DrawDiamond(rect, 0.5f, 0.52f, 0.26f, fill, shade);
            DrawMiniRect(rect, 0.47f, 0.44f, 0.06f, 0.17f, Color.Lerp(fill, Color.white, 0.5f));
        }

        private static void DrawKeyGlyph(Rect rect, Color tint)
        {
            Color fill = Color.Lerp(tint, Color.white, 0.36f);
            Color shade = Color.Lerp(tint, Color.black, 0.36f);
            DrawMiniRect(rect, 0.28f, 0.46f, 0.45f, 0.11f, fill);
            DrawMiniRect(rect, 0.61f, 0.55f, 0.08f, 0.13f, fill);
            DrawMiniRect(rect, 0.7f, 0.55f, 0.08f, 0.1f, shade);
            DrawMiniRect(rect, 0.23f, 0.38f, 0.2f, 0.26f, shade);
            DrawMiniRect(rect, 0.28f, 0.43f, 0.1f, 0.16f, Color.Lerp(shade, Color.white, 0.45f));
        }

        private static void DrawBottleGlyph(Rect rect, Color tint)
        {
            Color glass = Color.Lerp(tint, Color.white, 0.45f);
            Color liquid = Color.Lerp(tint, Color.black, 0.18f);
            DrawMiniRect(rect, 0.43f, 0.22f, 0.14f, 0.16f, glass);
            DrawMiniRect(rect, 0.35f, 0.36f, 0.3f, 0.42f, glass);
            DrawMiniRect(rect, 0.38f, 0.5f, 0.24f, 0.24f, liquid);
            DrawMiniRect(rect, 0.43f, 0.25f, 0.14f, 0.05f, Color.Lerp(glass, Color.black, 0.3f));
        }

        private static void DrawUnknownGlyph(Rect rect, Color tint)
        {
            DrawDiamond(rect, 0.5f, 0.52f, 0.26f, Color.Lerp(tint, Color.white, 0.2f), Color.Lerp(tint, Color.black, 0.38f));
        }

        private static void DrawDiamond(Rect rect, float centerX, float centerY, float size, Color fill, Color shade)
        {
            DrawMiniRect(rect, centerX - size * 0.35f, centerY - size * 0.12f, size * 0.7f, size * 0.24f, shade);
            DrawMiniRect(rect, centerX - size * 0.24f, centerY - size * 0.28f, size * 0.48f, size * 0.56f, fill);
            DrawMiniRect(rect, centerX - size * 0.12f, centerY - size * 0.38f, size * 0.24f, size * 0.76f, Color.Lerp(fill, Color.white, 0.25f));
        }

        private static void DrawMiniRect(Rect outer, float x, float y, float width, float height, Color color)
        {
            Rect rect = new Rect(
                outer.x + outer.width * x,
                outer.y + outer.height * y,
                outer.width * width,
                outer.height * height);
            GUI.DrawTexture(rect, GameGuiStyles.GetTexture(color));
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
