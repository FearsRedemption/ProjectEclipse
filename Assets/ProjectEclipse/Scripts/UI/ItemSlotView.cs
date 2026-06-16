using ProjectEclipse.Equipment;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public static class ItemSlotView
    {
        private const float SlotSize = 46f;

        public static bool Draw(ItemDefinition item, int quantity, ItemHoverState hover, bool selected = false)
        {
            Rect rect = GUILayoutUtility.GetRect(SlotSize, SlotSize, GUILayout.Width(SlotSize), GUILayout.Height(SlotSize));
            Color oldColor = GUI.color;
            GUI.color = selected ? new Color(1f, 0.94f, 0.62f, 1f) : Color.white;
            GUI.Box(rect, string.Empty);
            GUI.color = oldColor;

            if (item != null)
            {
                Rect iconRect = new Rect(rect.x + 5f, rect.y + 5f, 32f, 32f);
                Texture icon = GetIconTexture(item);
                if (icon != null)
                {
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
                }
                else
                {
                    GUI.Label(iconRect, "MISSING\nICON");
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

                if (rect.Contains(Event.current.mousePosition))
                {
                    hover.Set(item, quantity);
                }
            }

            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        public static Texture GetIconTexture(ItemDefinition item)
        {
            return item != null && item.Icon != null ? item.Icon.texture : null;
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
