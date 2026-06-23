using System.Collections.Generic;
using ProjectEclipse.Crafting;
using ProjectEclipse.Equipment;
using ProjectEclipse.Furnace;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public class InventoryScreen
    {
        private readonly InventoryStore inventory;
        private readonly EquipmentController equipment;
        private readonly InventoryCraftingController inventoryCrafting;
        private readonly EquipmentPanel equipmentPanel;
        private readonly CraftingPortPanel craftingPortPanel;
        private readonly CraftingPanel craftingPanel;
        private readonly ItemGridView inventoryGrid = new ItemGridView();

        private InventoryTab selectedTab = InventoryTab.Equipment;
        private ItemDefinition selectedItem;
        private string feedback = string.Empty;

        public InventoryScreen(
            InventoryStore inventory,
            EquipmentController equipment,
            InventoryCraftingController inventoryCrafting,
            CraftingSystem crafting,
            FurnaceSystem furnace)
        {
            this.inventory = inventory;
            this.equipment = equipment;
            this.inventoryCrafting = inventoryCrafting;
            equipmentPanel = new EquipmentPanel(equipment);
            craftingPortPanel = new CraftingPortPanel(inventoryCrafting, furnace);
            craftingPanel = new CraftingPanel(crafting);
        }

        public void Draw(int windowId, ItemHoverState hover)
        {
            if (inventory == null)
            {
                GUILayout.Label("Inventory missing.");
                GUI.DragWindow();
                return;
            }

            GUILayout.BeginHorizontal();
            DrawLeftSide(hover);
            GUILayout.Space(14f);
            DrawRightSide(hover);
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(feedback))
            {
                GUILayout.Space(4f);
                GUILayout.Label(feedback);
            }

            GUI.DragWindow();
        }

        public void ResetCraftingTransientState()
        {
            if (craftingPanel != null)
            {
                craftingPanel.ResetCraftAmount();
            }
        }

        private void DrawLeftSide(ItemHoverState hover)
        {
            GUILayout.BeginVertical(GUILayout.Width(510f));
            GUILayout.Label("Character Equipment");
            equipmentPanel.Draw(hover);
            GUILayout.Space(8f);
            craftingPortPanel.DrawEquipmentSlots(hover);
            GUILayout.Space(8f);
            GUILayout.Label("Inventory Crafting");
            craftingPanel.DrawIntegrated(185f);
            GUILayout.EndVertical();
        }

        private void DrawRightSide(ItemHoverState hover)
        {
            GUILayout.BeginVertical(GUILayout.Width(470f));
            DrawTabs();
            GUILayout.Space(6f);
            DrawInventoryGrid(hover);
            GUILayout.Space(6f);
            DrawSelectedSummary();
            GUILayout.EndVertical();
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

        private void DrawInventoryGrid(ItemHoverState hover)
        {
            List<InventoryStack> stacks = inventory.GetSnapshot();
            ItemDefinition clicked;
            bool shiftHeld = Event.current != null && Event.current.shift;
            ItemSlotClick click = inventoryGrid.DrawClickable(stacks, hover, ItemMatchesSelectedTab, 430f, selectedItem, out clicked);
            if (click == ItemSlotClick.None || clicked == null)
            {
                return;
            }

            selectedItem = clicked;
            if (click == ItemSlotClick.Right || shiftHeld)
            {
                HandleEquipClick(clicked);
            }
        }

        private bool ItemMatchesSelectedTab(ItemDefinition item)
        {
            if (item == null)
            {
                return false;
            }

            switch (selectedTab)
            {
                case InventoryTab.Equipment:
                    return item.Category == ItemCategory.Weapon || item.Category == ItemCategory.Armor;
                case InventoryTab.Usable:
                    return item.Category == ItemCategory.Consumable;
                case InventoryTab.Materials:
                    return item.Category == ItemCategory.Material;
                case InventoryTab.Misc:
                    return item.Category == ItemCategory.CraftingPort
                        || item.Category == ItemCategory.Upgrade
                        || item.Category == ItemCategory.Furnace
                        || item.Category == ItemCategory.Placeholder;
                case InventoryTab.KeyItems:
                    return item.Category == ItemCategory.KeyItem || item.Category == ItemCategory.Quest;
                default:
                    return true;
            }
        }

        private void HandleEquipClick(ItemDefinition item)
        {
            if (item == null)
            {
                return;
            }

            EquipmentDefinition gear = item as EquipmentDefinition;
            if (gear != null)
            {
                bool equipped = equipment != null && equipment.TryEquipFromStorage(gear);
                feedback = equipped ? "Equipped " + gear.DisplayName + "." : "Could not equip " + gear.DisplayName + ".";
                return;
            }

            CraftingPortDefinition port = item as CraftingPortDefinition;
            if (port != null)
            {
                bool equipped = inventoryCrafting != null && inventoryCrafting.TryEquipPortFromStorage(port);
                feedback = equipped ? "Equipped " + port.DisplayName + "." : "Could not equip " + port.DisplayName + ".";
                return;
            }

            feedback = item.DisplayName + " selected.";
        }

        private void DrawSelectedSummary()
        {
            if (selectedItem == null)
            {
                GUILayout.Label("Selected: None");
                return;
            }

            GUILayout.Label("Selected: " + selectedItem.DisplayName);
        }
    }
}
