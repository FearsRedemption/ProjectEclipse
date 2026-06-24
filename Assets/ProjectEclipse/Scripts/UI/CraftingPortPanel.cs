using ProjectEclipse.Crafting;
using ProjectEclipse.Furnace;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public class CraftingPortPanel
    {
        private readonly InventoryCraftingController inventoryCrafting;
        private readonly FurnaceSystem furnace;

        public CraftingPortPanel(InventoryCraftingController inventoryCrafting, FurnaceSystem furnace)
        {
            this.inventoryCrafting = inventoryCrafting;
            this.furnace = furnace;
        }

        public void Draw(int windowId)
        {
            DrawEquipmentSlots(new ItemHoverState());
            DrawLegacyFurnaceStatus();
            GUI.DragWindow();
        }

        public void DrawEquipmentSlots(ItemHoverState hover)
        {
            if (inventoryCrafting != null)
            {
                GUILayout.Label(CraftingTerminology.TrinketPlural);
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

        public void DrawLegacyFurnaceStatus()
        {
            if (furnace == null)
            {
                return;
            }

            GUILayout.Space(6f);
            GUILayout.Label("Legacy Furnace Station");
            GUILayout.Label("Tier: " + CraftingTrinketTierUtility.FormatTier(furnace.FurnaceLevel));
            GUILayout.Label("Fuel: " + DescribeSlot(furnace.FuelSlot));
            GUILayout.Label("Input: " + DescribeSlot(furnace.InputSlot));
            GUILayout.Label("Output: " + DescribeSlot(furnace.OutputSlot));
            Rect bar = GUILayoutUtility.GetRect(280f, 16f);
            GUI.Box(bar, string.Empty);
            GUI.Box(new Rect(bar.x, bar.y, bar.width * furnace.Progress01, bar.height), string.Empty);
        }

        private void DrawPortSlot(CraftingPortSlot slot, string label, ItemHoverState hover)
        {
            GUILayout.BeginVertical(GUILayout.Width(58f));
            GUILayout.Label(label);
            CraftingPortDefinition port = inventoryCrafting != null ? inventoryCrafting.GetEquippedPort(slot) : null;
            ItemSlotClick click = ItemSlotView.DrawCraftingPortSlot(port, port != null ? 1 : 0, hover, slot, label, port != null);
            if (click == ItemSlotClick.Right && port != null && inventoryCrafting != null)
            {
                inventoryCrafting.TryUnequipPort(slot);
            }
            GUILayout.EndVertical();
        }

        private static string DescribeSlot(FurnaceSlot slot)
        {
            if (slot == null || slot.IsEmpty || slot.Item == null)
            {
                return "Empty";
            }

            return slot.Item.DisplayName + " x" + slot.Quantity;
        }
    }
}
