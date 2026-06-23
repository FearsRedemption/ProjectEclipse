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
                GUILayout.Label("Crafting Ports");
                GUILayout.BeginHorizontal();
                DrawPortSlot(CraftingPortSlot.FurnacePort, "Furnace", hover);
                DrawPortSlot(CraftingPortSlot.CauldronPort, "Cauldron", hover);
                DrawPortSlot(CraftingPortSlot.ForgePort, "Forge", hover);
                DrawPortSlot(CraftingPortSlot.AnvilPort, "Anvil", hover);
                DrawPortSlot(CraftingPortSlot.UtilityPort, "Utility", hover);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("Inventory crafting ports not wired.");
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
            GUILayout.Label("Level: " + furnace.FurnaceLevel);
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
