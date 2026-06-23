using ProjectEclipse.Equipment;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public class EquipmentPanel
    {
        private readonly EquipmentController equipment;

        public EquipmentPanel(EquipmentController equipment)
        {
            this.equipment = equipment;
        }

        public void Draw(ItemHoverState hover)
        {
            GUILayout.BeginHorizontal();
            DrawLeftSlots(hover);
            DrawCharacterPreview();
            DrawRightSlots(hover);
            GUILayout.EndHorizontal();
        }

        private void DrawLeftSlots(ItemHoverState hover)
        {
            GUILayout.BeginVertical(GUILayout.Width(112f));
            DrawSlot(EquipmentSlot.Helmet, "Helmet", hover);
            DrawSlot(EquipmentSlot.Chest, "Chest", hover);
            DrawSlot(EquipmentSlot.Gloves, "Gloves", hover);
            DrawSlot(EquipmentSlot.Boots, "Boots", hover);
            DrawSlot(EquipmentSlot.Back, "Back", hover);
            GUILayout.EndVertical();
        }

        private void DrawCharacterPreview()
        {
            GUILayout.BeginVertical(GUILayout.Width(140f));
            GUILayout.Label("Warrior");
            Rect preview = GUILayoutUtility.GetRect(126f, 170f);
            GUI.Box(preview, "Character\nPreview\n\nLayered visuals\nneed Unity anchors");
            GUILayout.EndVertical();
        }

        private void DrawRightSlots(ItemHoverState hover)
        {
            GUILayout.BeginVertical(GUILayout.Width(180f));
            DrawSlot(EquipmentSlot.Mainhand, "Mainhand", hover);
            DrawSlot(EquipmentSlot.Offhand, "Offhand", hover);
            DrawSlot(EquipmentSlot.Necklace, "Necklace", hover);
            DrawSlot(EquipmentSlot.Ring1, "Ring 1", hover);
            DrawSlot(EquipmentSlot.Ring2, "Ring 2", hover);
            DrawSlot(EquipmentSlot.Earring1, "Earring 1", hover);
            DrawSlot(EquipmentSlot.Earring2, "Earring 2", hover);
            DrawSlot(EquipmentSlot.Belt, "Belt", hover);
            GUILayout.EndVertical();
        }

        private void DrawSlot(EquipmentSlot slot, string label, ItemHoverState hover)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(68f));
            ItemDefinition item = equipment != null ? equipment.GetEquippedItem(slot) : null;
            ItemSlotClick click = ItemSlotView.DrawEquipmentSlot(item, item != null ? 1 : 0, hover, slot, label, item != null);
            if (click == ItemSlotClick.Right && item != null && equipment != null)
            {
                equipment.TryUnequipToStorage(slot);
            }
            GUILayout.EndHorizontal();
        }
    }
}
