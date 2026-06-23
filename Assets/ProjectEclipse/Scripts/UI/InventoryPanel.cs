using System.Collections.Generic;
using ProjectEclipse.Crafting;
using ProjectEclipse.Equipment;
using ProjectEclipse.Furnace;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public class InventoryPanel
    {
        private readonly InventoryStore inventory;
        private readonly EquipmentController equipment;
        private readonly InventoryCraftingController inventoryCrafting;
        private readonly FurnaceSystem furnace;
        private readonly EquipmentPanel equipmentPanel;
        private readonly ItemGridView itemGrid = new ItemGridView();
        private InventoryTab selectedTab = InventoryTab.Equipment;

        public InventoryPanel(
            InventoryStore inventory,
            EquipmentController equipment,
            InventoryCraftingController inventoryCrafting,
            FurnaceSystem furnace)
        {
            this.inventory = inventory;
            this.equipment = equipment;
            this.inventoryCrafting = inventoryCrafting;
            this.furnace = furnace;
            equipmentPanel = new EquipmentPanel(equipment);
        }

        public void Draw(int windowId, ItemHoverState hover)
        {
            if (inventory == null)
            {
                GUILayout.Label("Inventory missing.");
                return;
            }

            DrawTabs();
            GUILayout.Space(6f);

            switch (selectedTab)
            {
                case InventoryTab.Equipment:
                    equipmentPanel.Draw(hover);
                    DrawCarriedEquipment(hover);
                    break;
                case InventoryTab.Materials:
                    DrawGrid(hover, item => item.Category == ItemCategory.Material);
                    break;
                case InventoryTab.Usable:
                    DrawGrid(hover, item => item.Category == ItemCategory.Consumable);
                    break;
                case InventoryTab.Misc:
                    DrawGrid(hover, item => item.Category == ItemCategory.Upgrade || item.Category == ItemCategory.CraftingPort || item.Category == ItemCategory.Furnace || item.Category == ItemCategory.Placeholder);
                    break;
                case InventoryTab.KeyItems:
                    DrawGrid(hover, item => item.Category == ItemCategory.KeyItem || item.Category == ItemCategory.Quest);
                    break;
            }

            GUI.DragWindow();
        }

        private void DrawTabs()
        {
            GUILayout.BeginHorizontal();
            TabButton(InventoryTab.Equipment, "Equipment");
            TabButton(InventoryTab.Usable, "Usable");
            TabButton(InventoryTab.Materials, "Materials");
            TabButton(InventoryTab.Misc, "Misc");
            TabButton(InventoryTab.KeyItems, "Key Items");
            GUILayout.EndHorizontal();
        }

        private void TabButton(InventoryTab tab, string label)
        {
            GUI.enabled = selectedTab != tab;
            if (GUILayout.Button(label, GUILayout.Height(28f)))
            {
                selectedTab = tab;
            }
            GUI.enabled = true;
        }

        private void DrawCarriedEquipment(ItemHoverState hover)
        {
            GUILayout.Label("Carried Equipment");
            DrawGrid(hover, item => item.Category == ItemCategory.Weapon || item.Category == ItemCategory.Armor, 90f);
        }

        private void DrawGrid(ItemHoverState hover, System.Predicate<ItemDefinition> filter, float height = 255f)
        {
            List<InventoryStack> stacks = inventory.GetSnapshot();
            ItemDefinition clicked;
            bool shiftHeld = Event.current != null && Event.current.shift;
            ItemSlotClick click = itemGrid.DrawClickable(stacks, hover, filter, height, null, out clicked);
            if (clicked != null && (click == ItemSlotClick.Right || shiftHeld))
            {
                HandleClick(clicked);
            }
        }

        private void HandleClick(ItemDefinition item)
        {
            if (item == null)
            {
                return;
            }

            if ((item.Category == ItemCategory.Weapon || item.Category == ItemCategory.Armor) && equipment != null)
            {
                equipment.TryEquipFromStorage(item);
                return;
            }

            CraftingPortDefinition port = item as CraftingPortDefinition;
            if (port != null && inventoryCrafting != null)
            {
                inventoryCrafting.TryEquipPort(port);
                return;
            }

            if (furnace != null && (item.ItemId.Contains("coal") || item.ItemId.Contains("copper")))
            {
                furnace.TryMoveToRelevantSlot(item, 1);
            }
        }
    }
}
