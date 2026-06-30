using System.Collections.Generic;
using ProjectEclipse.Crafting;
using ProjectEclipse.Equipment;
using ProjectEclipse.Input;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using ProjectEclipse.Player;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public class InventoryScreen
    {
        private const float ManualDropPickupLockoutSeconds = 4f;
        private const float ManualDropMagnetLockoutSeconds = 5f;
        private const float ManualDropLifetimeSeconds = 120f;
        private const float DefaultContentWidth = 1084f;
        private const float DefaultContentHeight = 668f;
        private const float LeftColumnWidth = 520f;
        private const float RightColumnWidth = 512f;
        private const float ColumnGap = 16f;
        private const float EquipmentPanelHeight = 292f;
        private const float SelectedSummaryHeight = 76f;

        private readonly InventoryStore inventory;
        private readonly EquipmentController equipment;
        private readonly InventoryCraftingController inventoryCrafting;
        private readonly EquipmentPanel equipmentPanel;
        private readonly CraftingPortPanel craftingPortPanel;
        private readonly CraftingPanel craftingPanel;
        private DropSpawner dropSpawner;
        private readonly ItemGridView inventoryGrid = new ItemGridView();

        private InventoryTab selectedTab = InventoryTab.Equipment;
        private ItemDefinition selectedItem;
        private string feedback = string.Empty;
        private float lastManualDropTime;

        public InventoryScreen(
            InventoryStore inventory,
            EquipmentController equipment,
            InventoryCraftingController inventoryCrafting,
            CraftingSystem crafting,
            DropSpawner dropSpawner = null)
        {
            this.inventory = inventory;
            this.equipment = equipment;
            this.inventoryCrafting = inventoryCrafting;
            this.dropSpawner = dropSpawner;
            equipmentPanel = new EquipmentPanel(equipment);
            craftingPortPanel = new CraftingPortPanel(inventoryCrafting);
            craftingPanel = new CraftingPanel(crafting);
        }

        public void Draw(int windowId, ItemHoverState hover)
        {
            Draw(windowId, hover, DefaultContentWidth, DefaultContentHeight);
        }

        public void Draw(int windowId, ItemHoverState hover, float availableWidth, float availableHeight)
        {
            GameGuiStyles.ApplySkin(GUI.skin);
            if (inventory == null)
            {
                GUILayout.Label("Inventory missing.");
                return;
            }

            float contentWidth = Mathf.Max(DefaultContentWidth, availableWidth);
            float contentHeight = Mathf.Max(DefaultContentHeight, availableHeight);
            float innerHeight = Mathf.Max(560f, contentHeight - 24f);
            float bodyHeight = !string.IsNullOrEmpty(feedback) ? innerHeight - 28f : innerHeight;

            GUILayout.BeginVertical(GameGuiStyles.InventorySurface, GUILayout.Width(contentWidth), GUILayout.Height(contentHeight));
            GUILayout.BeginHorizontal(GUILayout.Height(bodyHeight));
            DrawLeftSide(hover, LeftColumnWidth, bodyHeight);
            GUILayout.Space(ColumnGap);
            DrawRightSide(hover, RightColumnWidth, bodyHeight);
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(feedback))
            {
                GUILayout.Space(4f);
                GUILayout.Label(feedback);
            }
            GUILayout.EndVertical();
        }

        public void ResetCraftingTransientState()
        {
            if (craftingPanel != null)
            {
                craftingPanel.ResetCraftAmount();
            }
        }

        private void DrawLeftSide(ItemHoverState hover, float width, float height)
        {
            float craftingHeight = Mathf.Max(210f, height - EquipmentPanelHeight - 132f);
            GUILayout.BeginVertical(GameGuiStyles.SubPanel, GUILayout.Width(width), GUILayout.Height(height));
            GUILayout.Label("Character Equipment", GameGuiStyles.HeaderLabel);
            equipmentPanel.Draw(hover);
            GUILayout.Space(8f);
            craftingPortPanel.DrawEquipmentSlots(hover);
            GUILayout.Space(8f);
            GUILayout.Label("Inventory Crafting", GameGuiStyles.HeaderLabel);
            craftingPanel.DrawIntegrated(craftingHeight);
            GUILayout.EndVertical();
        }

        private void DrawRightSide(ItemHoverState hover, float width, float height)
        {
            float gridHeight = Mathf.Max(300f, height - SelectedSummaryHeight - 86f);
            GUILayout.BeginVertical(GameGuiStyles.SubPanel, GUILayout.Width(width), GUILayout.Height(height));
            DrawTabs();
            GUILayout.Space(6f);
            DrawInventoryGrid(hover, gridHeight);
            DrawInventoryCount();
            GUILayout.Space(6f);
            DrawSelectedSummary();
            GUILayout.EndVertical();
        }

        public void HandlePendingDragDrop(Rect inventoryWindowRect)
        {
            if (!inventoryGrid.IsDraggingItem)
            {
                return;
            }

            DrawDraggedItemPreview();
            Event current = Event.current;
            if (current == null || current.rawType != EventType.MouseUp)
            {
                return;
            }

            Vector2 pointer = GetPointerGuiPosition();
            ItemDefinition item;
            int quantity;
            if (!inventoryGrid.TryTakeDraggedItem(out item, out quantity))
            {
                return;
            }

            if (inventoryWindowRect.Contains(pointer))
            {
                selectedItem = item;
                return;
            }

            DropDraggedItem(item, quantity);
            current.Use();
        }

        private void DrawTabs()
        {
            GUILayout.BeginHorizontal();
            TabButton(InventoryTab.Equipment, "Equipment");
            TabButton(InventoryTab.Usable, "Usable / Consum.");
            TabButton(InventoryTab.Materials, "Materials");
            TabButton(InventoryTab.Misc, "Misc");
            TabButton(InventoryTab.KeyItems, "Key Items");
            GUILayout.EndHorizontal();
            if (selectedTab == InventoryTab.Equipment)
            {
                GUILayout.Label("Gear, accessories, weapons, armor, and crafting trinkets", GameGuiStyles.MutedLabel);
            }
        }

        private void TabButton(InventoryTab tab, string label)
        {
            GUIStyle style = selectedTab == tab ? GameGuiStyles.SelectedButton : GameGuiStyles.Button;
            if (GUILayout.Button(label, style, GUILayout.Height(28f)))
            {
                selectedTab = tab;
            }
        }

        private void DrawInventoryGrid(ItemHoverState hover, float height)
        {
            List<InventoryStack> stacks = inventory.GetSnapshot();
            ItemDefinition clicked;
            bool shiftHeld = Event.current != null && Event.current.shift;
            ItemSlotClick click = inventoryGrid.DrawClickable(stacks, hover, ItemMatchesSelectedTab, height, selectedItem, out clicked);
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
                    return item is EquipmentDefinition
                        || item is CraftingPortDefinition
                        || item.Category == ItemCategory.Weapon
                        || item.Category == ItemCategory.Armor
                        || item.Category == ItemCategory.CraftingPort;
                case InventoryTab.Usable:
                    return item.Category == ItemCategory.Consumable;
                case InventoryTab.Materials:
                    return item.Category == ItemCategory.Material;
                case InventoryTab.Misc:
                    return (!(item is EquipmentDefinition) && item.Category == ItemCategory.Upgrade)
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
                feedback = equipped ? "Socketed " + port.DisplayName + "." : "Could not socket " + port.DisplayName + ".";
                return;
            }

            feedback = item.DisplayName + " selected.";
        }

        private void DrawInventoryCount()
        {
            string label = "Visible: " + inventoryGrid.LastVisibleCount + " / Total: " + inventoryGrid.LastTotalCount;
            GUILayout.Label(label, GameGuiStyles.MutedLabel);
        }

        private void DrawSelectedSummary()
        {
            Rect summary = GUILayoutUtility.GetRect(480f, SelectedSummaryHeight, GUILayout.Width(480f), GUILayout.Height(SelectedSummaryHeight));
            GameGuiStyles.DrawBox(summary, new Color(0.1f, 0.13f, 0.14f, 0.96f), new Color(0.33f, 0.4f, 0.39f, 1f), 1f);
            if (selectedItem == null)
            {
                GUI.Label(new Rect(summary.x + 10f, summary.y + 8f, summary.width - 20f, 20f), "Selected: None", GameGuiStyles.HeaderLabel);
                return;
            }

            GUI.Label(new Rect(summary.x + 10f, summary.y + 7f, summary.width - 20f, 20f), selectedItem.DisplayName, GameGuiStyles.HeaderLabel);
            GUI.Label(new Rect(summary.x + 10f, summary.y + 30f, summary.width - 20f, 17f), selectedItem.Category + " / " + selectedItem.ResourceTier, GameGuiStyles.SmallLabel);
            string detail = !string.IsNullOrEmpty(selectedItem.CraftingUsage) ? selectedItem.CraftingUsage : selectedItem.Description;
            if (string.IsNullOrEmpty(detail))
            {
                detail = "No additional item notes.";
            }
            GUI.Label(new Rect(summary.x + 10f, summary.y + 49f, summary.width - 20f, 20f), detail, GameGuiStyles.MutedLabel);
        }

        private void DrawDraggedItemPreview()
        {
            ItemDefinition item = inventoryGrid.DraggingItem;
            if (item == null)
            {
                return;
            }

            Vector2 pointer = GetPointerGuiPosition();
            Rect rect = new Rect(pointer.x + 12f, pointer.y + 12f, ItemSlotView.SlotSize, ItemSlotView.SlotSize);
            ItemSlotView.DrawClick(rect, item, 1, null, false);
        }

        private void DropDraggedItem(ItemDefinition item, int visibleStackQuantity)
        {
            if (item == null || inventory == null || Time.time - lastManualDropTime < 0.08f)
            {
                return;
            }

            int quantity = Event.current != null && Event.current.shift ? Mathf.Max(1, visibleStackQuantity) : 1;
            if (!inventory.RemoveItem(item, quantity))
            {
                feedback = "Could not drop " + item.DisplayName + ".";
                return;
            }

            if (dropSpawner == null)
            {
                dropSpawner = Object.FindAnyObjectByType<DropSpawner>();
            }

            if (dropSpawner == null)
            {
                inventory.AddItem(item, quantity);
                feedback = "Could not find a world drop spawner.";
                return;
            }

            WorldItemDrop dropped = dropSpawner.SpawnDrop(item, quantity, GetManualDropPosition());
            if (dropped != null)
            {
                dropped.ConfigureManualDropSafety(inventory, ManualDropPickupLockoutSeconds, ManualDropMagnetLockoutSeconds, ManualDropLifetimeSeconds);
            }

            lastManualDropTime = Time.time;
            feedback = "Dropped " + item.DisplayName + (quantity > 1 ? " x" + quantity : string.Empty) + ".";
        }

        private Vector3 GetManualDropPosition()
        {
            Transform origin = inventory != null ? inventory.transform : null;
            if (origin == null)
            {
                return Vector3.zero;
            }

            int direction = 1;
            PlayerController player = origin.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                direction = player.FacingDirection;
            }
            else if (Mathf.Abs(origin.lossyScale.x) > 0.001f)
            {
                direction = origin.lossyScale.x >= 0f ? 1 : -1;
            }

            return origin.position + new Vector3(direction * 0.9f, 0.65f, 0f);
        }

        private static Vector2 GetPointerGuiPosition()
        {
            Vector2 mouse = GameInput.PointerScreenPosition;
            return new Vector2(mouse.x, Screen.height - mouse.y);
        }
    }
}
