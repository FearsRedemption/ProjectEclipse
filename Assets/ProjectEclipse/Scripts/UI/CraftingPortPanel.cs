using ProjectEclipse.Crafting;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public class CraftingPortPanel
    {
        private readonly InventoryCraftingController inventoryCrafting;

        public CraftingPortPanel(InventoryCraftingController inventoryCrafting)
        {
            this.inventoryCrafting = inventoryCrafting;
        }

        public void Draw(int windowId)
        {
            GameGuiStyles.ApplySkin(GUI.skin);
            DrawEquipmentSlots(new ItemHoverState());
            GUI.DragWindow();
        }

        public void DrawEquipmentSlots(ItemHoverState hover)
        {
            GameGuiStyles.ApplySkin(GUI.skin);
            if (inventoryCrafting != null)
            {
                GUILayout.Label(CraftingTerminology.TrinketPlural, GameGuiStyles.HeaderLabel);
                GUILayout.BeginHorizontal();
                DrawPortSlot(CraftingPortSlot.WorkbenchPort, CraftingTerminology.GetSlotDisplayName(CraftingPortSlot.WorkbenchPort), hover);
                DrawPortSlot(CraftingPortSlot.UtilityPort, CraftingTerminology.GetSlotDisplayName(CraftingPortSlot.UtilityPort), hover);
                DrawPortSlot(CraftingPortSlot.FurnacePort, CraftingTerminology.GetSlotDisplayName(CraftingPortSlot.FurnacePort), hover);
                DrawPortSlot(CraftingPortSlot.AnvilPort, CraftingTerminology.GetSlotDisplayName(CraftingPortSlot.AnvilPort), hover);
                DrawPortSlot(CraftingPortSlot.CauldronPort, CraftingTerminology.GetSlotDisplayName(CraftingPortSlot.CauldronPort), hover);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("Inventory crafting trinkets not wired.");
            }
        }

        private void DrawPortSlot(CraftingPortSlot slot, string label, ItemHoverState hover)
        {
            Rect cell = GUILayoutUtility.GetRect(90f, 72f, GUILayout.Width(90f), GUILayout.Height(72f));
            GameGuiStyles.DrawInsetPanel(cell);
            GUI.Label(new Rect(cell.x + 4f, cell.y + 3f, cell.width - 8f, 16f), label, GameGuiStyles.CenterLabel);
            CraftingPortDefinition port = inventoryCrafting != null ? inventoryCrafting.GetEquippedPort(slot) : null;
            Rect slotRect = new Rect(cell.x + (cell.width - ItemSlotView.SlotSize) * 0.5f, cell.y + 20f, ItemSlotView.SlotSize, ItemSlotView.SlotSize);
            ItemSlotClick click = ItemSlotView.DrawCraftingPortSlot(slotRect, port, port != null ? 1 : 0, hover, slot, label, port != null);
            if (click == ItemSlotClick.Right && port != null && inventoryCrafting != null)
            {
                inventoryCrafting.TryUnequipPort(slot);
            }
        }

    }
}
