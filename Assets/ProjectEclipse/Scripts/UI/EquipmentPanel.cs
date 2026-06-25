using ProjectEclipse.Equipment;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public class EquipmentPanel
    {
        private const float SlotRowHeight = 58f;

        private readonly EquipmentController equipment;

        public EquipmentPanel(EquipmentController equipment)
        {
            this.equipment = equipment;
        }

        public void Draw(ItemHoverState hover)
        {
            GameGuiStyles.ApplySkin(GUI.skin);
            GUILayout.BeginHorizontal(GUILayout.Height(292f));
            DrawLeftSlots(hover);
            GUILayout.Space(8f);
            DrawCharacterPreview();
            GUILayout.Space(8f);
            DrawRightSlots(hover);
            GUILayout.EndHorizontal();
        }

        private void DrawLeftSlots(ItemHoverState hover)
        {
            GUILayout.BeginVertical(GUILayout.Width(154f));
            DrawSlot(EquipmentSlot.Helmet, "Helmet", hover);
            DrawSlot(EquipmentSlot.Chest, "Chest", hover);
            DrawSlot(EquipmentSlot.Gloves, "Gloves", hover);
            DrawSlot(EquipmentSlot.Boots, "Boots", hover);
            DrawSlot(EquipmentSlot.Back, "Back", hover);
            GUILayout.EndVertical();
        }

        private void DrawCharacterPreview()
        {
            GUILayout.BeginVertical(GUILayout.Width(132f));
            GUILayout.Label("Warrior", GameGuiStyles.HeaderLabel);
            Rect preview = GUILayoutUtility.GetRect(132f, 210f, GUILayout.Width(132f), GUILayout.Height(210f));
            GameGuiStyles.DrawBox(preview, new Color(0.09f, 0.12f, 0.13f, 1f), new Color(0.35f, 0.42f, 0.4f, 1f), 1f);
            DrawPreviewSilhouette(preview);
            GUILayout.EndVertical();
        }

        private void DrawRightSlots(ItemHoverState hover)
        {
            GUILayout.BeginVertical(GUILayout.Width(176f));
            DrawSlot(EquipmentSlot.Mainhand, "Mainhand", hover);
            DrawSlot(EquipmentSlot.Offhand, "Offhand", hover);
            GUILayout.Space(4f);
            GUILayout.Label("Accessories", GameGuiStyles.SmallLabel);
            GUILayout.BeginHorizontal();
            DrawCompactSlot(EquipmentSlot.Necklace, "Neck", hover);
            DrawCompactSlot(EquipmentSlot.Belt, "Belt", hover);
            DrawCompactSlot(EquipmentSlot.Ring1, "R1", hover);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            DrawCompactSlot(EquipmentSlot.Ring2, "R2", hover);
            DrawCompactSlot(EquipmentSlot.Earring1, "E1", hover);
            DrawCompactSlot(EquipmentSlot.Earring2, "E2", hover);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawSlot(EquipmentSlot slot, string label, ItemHoverState hover)
        {
            Rect row = GUILayoutUtility.GetRect(150f, SlotRowHeight, GUILayout.Width(150f), GUILayout.Height(SlotRowHeight));
            GameGuiStyles.DrawBox(row, new Color(0.11f, 0.14f, 0.15f, 0.96f), new Color(0.29f, 0.36f, 0.35f, 1f), 1f);

            ItemDefinition item = equipment != null ? equipment.GetEquippedItem(slot) : null;
            GUI.Label(new Rect(row.x + 7f, row.y + 6f, 86f, 17f), label, GameGuiStyles.SmallLabel);
            string itemName = item != null ? item.DisplayName : "Empty";
            GUI.Label(new Rect(row.x + 7f, row.y + 26f, 88f, 24f), itemName, GameGuiStyles.MutedLabel);

            Rect slotRect = new Rect(row.x + row.width - ItemSlotView.SlotSize - 5f, row.y + 5f, ItemSlotView.SlotSize, ItemSlotView.SlotSize);
            ItemSlotClick click = ItemSlotView.DrawEquipmentSlot(slotRect, item, item != null ? 1 : 0, hover, slot, label, item != null);
            if (click == ItemSlotClick.Right && item != null && equipment != null)
            {
                equipment.TryUnequipToStorage(slot);
            }
        }

        private void DrawCompactSlot(EquipmentSlot slot, string label, ItemHoverState hover)
        {
            Rect cell = GUILayoutUtility.GetRect(56f, 66f, GUILayout.Width(56f), GUILayout.Height(66f));
            GameGuiStyles.DrawBox(cell, new Color(0.11f, 0.14f, 0.15f, 0.96f), new Color(0.29f, 0.36f, 0.35f, 1f), 1f);
            GUI.Label(new Rect(cell.x + 4f, cell.y + 3f, cell.width - 8f, 15f), label, GameGuiStyles.CenterLabel);

            ItemDefinition item = equipment != null ? equipment.GetEquippedItem(slot) : null;
            Rect slotRect = new Rect(cell.x + (cell.width - ItemSlotView.SlotSize) * 0.5f, cell.y + 17f, ItemSlotView.SlotSize, ItemSlotView.SlotSize);
            ItemSlotClick click = ItemSlotView.DrawEquipmentSlot(slotRect, item, item != null ? 1 : 0, hover, slot, label, item != null);
            if (click == ItemSlotClick.Right && item != null && equipment != null)
            {
                equipment.TryUnequipToStorage(slot);
            }
        }

        private static void DrawPreviewSilhouette(Rect preview)
        {
            Color body = new Color(0.72f, 0.78f, 0.72f, 1f);
            Color trim = new Color(0.3f, 0.38f, 0.36f, 1f);
            Rect head = new Rect(preview.x + preview.width * 0.42f, preview.y + 28f, preview.width * 0.16f, 24f);
            Rect torso = new Rect(preview.x + preview.width * 0.37f, preview.y + 58f, preview.width * 0.26f, 64f);
            Rect legs = new Rect(preview.x + preview.width * 0.39f, preview.y + 122f, preview.width * 0.22f, 56f);
            Rect blade = new Rect(preview.x + preview.width * 0.63f, preview.y + 68f, 8f, 76f);

            GUI.DrawTexture(head, GameGuiStyles.GetTexture(body));
            GUI.DrawTexture(torso, GameGuiStyles.GetTexture(body));
            GUI.DrawTexture(legs, GameGuiStyles.GetTexture(body));
            GUI.DrawTexture(blade, GameGuiStyles.GetTexture(new Color(0.85f, 0.86f, 0.8f, 1f)));
            GUI.DrawTexture(new Rect(torso.x, torso.y + 28f, torso.width, 6f), GameGuiStyles.GetTexture(trim));
            GUI.DrawTexture(new Rect(preview.x + 18f, preview.y + preview.height - 28f, preview.width - 36f, 2f), GameGuiStyles.GetTexture(new Color(0.42f, 0.5f, 0.48f, 1f)));
        }
    }
}
