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
            if (inventoryCrafting != null)
            {
                GUILayout.Label("Equipped Ports");
                for (int i = 0; i < inventoryCrafting.EquippedPorts.Count; i++)
                {
                    CraftingPortDefinition port = inventoryCrafting.EquippedPorts[i];
                    if (port != null)
                    {
                        GUILayout.Label(port.PortSlot + ": " + port.DisplayName);
                    }
                }
            }
            else
            {
                GUILayout.Label("Inventory crafting ports not wired.");
            }

            if (furnace != null)
            {
                GUILayout.Space(6f);
                GUILayout.Label("Legacy Furnace");
                GUILayout.Label("Level: " + furnace.FurnaceLevel);
                GUILayout.Label("Fuel: " + DescribeSlot(furnace.FuelSlot));
                GUILayout.Label("Input: " + DescribeSlot(furnace.InputSlot));
                GUILayout.Label("Output: " + DescribeSlot(furnace.OutputSlot));
                Rect bar = GUILayoutUtility.GetRect(280f, 16f);
                GUI.Box(bar, string.Empty);
                GUI.Box(new Rect(bar.x, bar.y, bar.width * furnace.Progress01, bar.height), string.Empty);
            }

            GUI.DragWindow();
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
