using System.Collections.Generic;
using ProjectEclipse.Combat;
using ProjectEclipse.Crafting;
using ProjectEclipse.Equipment;
using ProjectEclipse.Furnace;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public class MvpHud : MonoBehaviour
    {
        private Health playerHealth;
        private InventoryStore inventory;
        private EquipmentController equipment;
        private CraftingSystem crafting;
        private InventoryCraftingController inventoryCrafting;
        private FurnaceSystem furnace;
        private Vector2 inventoryScroll;
        private bool inventoryOpen;
        private InventoryTab selectedTab = InventoryTab.Materials;
        private ItemDefinition hoveredItem;
        private int hoveredQuantity;

        public void Initialize(
            Health health,
            InventoryStore store,
            EquipmentController playerEquipment,
            CraftingSystem craftingSystem,
            FurnaceSystem furnaceSystem)
        {
            playerHealth = health;
            inventory = store;
            equipment = playerEquipment;
            crafting = craftingSystem;
            inventoryCrafting = store != null ? store.GetComponent<InventoryCraftingController>() : null;
            furnace = furnaceSystem;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                inventoryOpen = !inventoryOpen;
            }
        }

        private void OnGUI()
        {
            GUI.skin.window.fontSize = 14;
            GUI.skin.label.fontSize = 13;
            GUI.skin.button.fontSize = 13;

            GUILayout.Window(1, new Rect(12f, 12f, 280f, 120f), DrawStatusWindow, "Status");
            if (!inventoryOpen)
            {
                return;
            }

            hoveredItem = null;
            hoveredQuantity = 0;

            GUILayout.Window(2, new Rect(12f, 140f, 420f, 360f), DrawInventoryWindow, "Inventory");
            GUILayout.Window(3, new Rect(Screen.width - 328f, 12f, 316f, 240f), DrawCraftingWindow, "Crafting");
            GUILayout.Window(4, new Rect(Screen.width - 328f, 260f, 316f, 170f), DrawFurnaceWindow, "Furnace");
            if (hoveredItem != null)
            {
                DrawTooltip(hoveredItem, hoveredQuantity);
            }
        }

        private void DrawStatusWindow(int windowId)
        {
            if (playerHealth != null)
            {
                GUILayout.Label("Health: " + playerHealth.CurrentHealth + " / " + playerHealth.MaxHealth);
                Rect bar = GUILayoutUtility.GetRect(240f, 16f);
                GUI.Box(bar, string.Empty);
                float fill = playerHealth.MaxHealth > 0 ? (float)playerHealth.CurrentHealth / playerHealth.MaxHealth : 0f;
                GUI.Box(new Rect(bar.x, bar.y, bar.width * fill, bar.height), string.Empty);
            }

            string weaponName = equipment != null && equipment.EquippedWeapon != null ? equipment.EquippedWeapon.DisplayName : "None";
            GUILayout.Label("Weapon: " + weaponName);
            GUI.DragWindow();
        }

        private void DrawInventoryWindow(int windowId)
        {
            if (inventory == null)
            {
                GUILayout.Label("Storage missing.");
                return;
            }

            DrawInventoryTabs();
            GUILayout.Space(6f);

            switch (selectedTab)
            {
                case InventoryTab.Equipment:
                    DrawEquipmentTab();
                    break;
                case InventoryTab.Materials:
                    DrawItemGrid(ItemCategory.Material);
                    break;
                case InventoryTab.Consumables:
                    DrawItemGrid(ItemCategory.Consumable);
                    break;
                case InventoryTab.KeyItems:
                    DrawKeyItemsTab();
                    break;
            }

            GUI.DragWindow();
        }

        private void DrawInventoryTabs()
        {
            GUILayout.BeginHorizontal();
            DrawTabButton(InventoryTab.Equipment, "Equipment");
            DrawTabButton(InventoryTab.Materials, "Materials");
            DrawTabButton(InventoryTab.Consumables, "Consumables");
            DrawTabButton(InventoryTab.KeyItems, "Key Items");
            GUILayout.EndHorizontal();
        }

        private void DrawTabButton(InventoryTab tab, string label)
        {
            GUI.enabled = selectedTab != tab;
            if (GUILayout.Button(label, GUILayout.Height(28f)))
            {
                selectedTab = tab;
            }
            GUI.enabled = true;
        }

        private void DrawEquipmentTab()
        {
            GUILayout.Label("Warrior / Level 1");
            GUILayout.Label("Main Gear");
            DrawEquipmentSlot(EquipmentSlot.Mainhand, "Mainhand");
            DrawEquipmentSlot(EquipmentSlot.Offhand, "Offhand");
            DrawEquipmentSlot(EquipmentSlot.Helmet, "Helmet");
            DrawEquipmentSlot(EquipmentSlot.Chest, "Chest");
            DrawEquipmentSlot(EquipmentSlot.Boots, "Boots");
            DrawEquipmentSlot(EquipmentSlot.Gloves, "Gloves");

            GUILayout.Space(4f);
            GUILayout.Label("Accessories");
            DrawEquipmentSlot(EquipmentSlot.Necklace, "Necklace");
            DrawEquipmentSlot(EquipmentSlot.Ring1, "Ring 1");
            DrawEquipmentSlot(EquipmentSlot.Ring2, "Ring 2");
            DrawEquipmentSlot(EquipmentSlot.Earring1, "Earring 1");
            DrawEquipmentSlot(EquipmentSlot.Earring2, "Earring 2");
            DrawEquipmentSlot(EquipmentSlot.Belt, "Belt");
            DrawEquipmentSlot(EquipmentSlot.Back, "Back");

            GUILayout.Space(6f);
            GUILayout.Label("Carried Equipment");
            DrawEquipmentInventory();
        }

        private void DrawEquipmentSlot(EquipmentSlot slot, string label)
        {
            ItemDefinition item = equipment != null ? equipment.GetEquippedItem(slot) : null;
            Rect rect = GUILayoutUtility.GetRect(380f, 24f);
            GUI.Box(rect, label + ": " + (item != null ? item.DisplayName : "Empty"));
            if (item != null && rect.Contains(Event.current.mousePosition))
            {
                hoveredItem = item;
                hoveredQuantity = 1;
            }
        }

        private void DrawEquipmentInventory()
        {
            List<InventoryStack> stacks = inventory.GetSnapshot();
            inventoryScroll = GUILayout.BeginScrollView(inventoryScroll, GUILayout.Height(90f));
            for (int i = 0; i < stacks.Count; i++)
            {
                InventoryStack stack = stacks[i];
                if (stack.Item == null || (stack.Item.Category != ItemCategory.Weapon && stack.Item.Category != ItemCategory.Armor))
                {
                    continue;
                }

                if (DrawItemButton(stack.Item, stack.Quantity))
                {
                    HandleStorageClick(stack.Item, stack.Quantity, Event.current != null && Event.current.shift);
                }
            }
            GUILayout.EndScrollView();
        }

        private void DrawItemGrid(ItemCategory category)
        {
            List<InventoryStack> stacks = inventory.GetSnapshot();
            inventoryScroll = GUILayout.BeginScrollView(inventoryScroll, GUILayout.Height(255f));
            for (int i = 0; i < stacks.Count; i++)
            {
                InventoryStack stack = stacks[i];
                if (stack.Item == null || stack.Item.Category != category)
                {
                    continue;
                }

                if (DrawItemButton(stack.Item, stack.Quantity))
                {
                    HandleStorageClick(stack.Item, stack.Quantity, Event.current != null && Event.current.shift);
                }
            }
            GUILayout.EndScrollView();
        }

        private bool DrawItemButton(ItemDefinition item, int quantity)
        {
            GUI.backgroundColor = item.PlaceholderColor;
            Rect rect = GUILayoutUtility.GetRect(380f, 34f);
            bool clicked = GUI.Button(rect, item.DisplayName + " x" + quantity + "  [" + item.ResourceTier + "]");
            GUI.backgroundColor = Color.white;
            if (rect.Contains(Event.current.mousePosition))
            {
                hoveredItem = item;
                hoveredQuantity = quantity;
            }
            return clicked;
        }

        private void DrawKeyItemsTab()
        {
            GUILayout.Label("Special items, key items, and crafting ports.");
            DrawSpecialItemGrid();
        }

        private void DrawSpecialItemGrid()
        {
            List<InventoryStack> stacks = inventory.GetSnapshot();
            inventoryScroll = GUILayout.BeginScrollView(inventoryScroll, GUILayout.Height(255f));
            for (int i = 0; i < stacks.Count; i++)
            {
                InventoryStack stack = stacks[i];
                if (stack.Item == null || (stack.Item.Category != ItemCategory.KeyItem && stack.Item.Category != ItemCategory.CraftingPort && stack.Item.Category != ItemCategory.Furnace))
                {
                    continue;
                }

                if (DrawItemButton(stack.Item, stack.Quantity))
                {
                    HandleStorageClick(stack.Item, stack.Quantity, Event.current != null && Event.current.shift);
                }
            }
            GUILayout.EndScrollView();
        }

        private void DrawCraftingWindow(int windowId)
        {
            if (crafting == null)
            {
                GUILayout.Label("Crafting missing.");
                return;
            }

            IReadOnlyList<CraftingRecipe> recipes = crafting.Recipes;
            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe recipe = recipes[i];
                GUI.enabled = crafting.CanCraft(recipe);
                if (GUILayout.Button(recipe.DisplayName))
                {
                    crafting.TryCraft(recipe);
                }
                GUI.enabled = true;
                GUILayout.Label(DescribeRecipe(recipe));
            }

            GUI.DragWindow();
        }

        private void DrawFurnaceWindow(int windowId)
        {
            if (furnace == null)
            {
                GUILayout.Label("Furnace missing.");
                return;
            }

            GUILayout.Label("Level: " + furnace.FurnaceLevel);
            GUILayout.Label("Fuel: " + DescribeSlot(furnace.FuelSlot));
            GUILayout.Label("Input: " + DescribeSlot(furnace.InputSlot));
            GUILayout.Label("Output: " + DescribeSlot(furnace.OutputSlot));
            Rect bar = GUILayoutUtility.GetRect(260f, 16f);
            GUI.Box(bar, string.Empty);
            GUI.Box(new Rect(bar.x, bar.y, bar.width * furnace.Progress01, bar.height), string.Empty);
            GUI.DragWindow();
        }

        private void HandleStorageClick(ItemDefinition item, int quantity, bool shift)
        {
            if (!shift || item == null)
            {
                return;
            }

            if (item.Category == ItemCategory.Weapon && equipment != null)
            {
                equipment.TryEquipFromStorage(item);
                return;
            }

            if (item.Category == ItemCategory.Armor && equipment != null)
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
                furnace.TryMoveToRelevantSlot(item, Mathf.Min(quantity, 1));
            }
        }

        private static string DescribeRecipe(CraftingRecipe recipe)
        {
            if (recipe == null)
            {
                return string.Empty;
            }

            List<string> parts = new List<string>();
            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                CraftingIngredient ingredient = recipe.Ingredients[i];
                if (ingredient != null && ingredient.Item != null)
                {
                    parts.Add(ingredient.Item.DisplayName + " x" + ingredient.Quantity);
                }
            }

            string output = recipe.OutputItem != null ? recipe.OutputItem.DisplayName + " x" + recipe.OutputQuantity : "Unknown";
            return string.Join(", ", parts.ToArray()) + " -> " + output;
        }

        private void DrawTooltip(ItemDefinition item, int quantity)
        {
            Rect rect = new Rect(Event.current.mousePosition.x + 18f, Event.current.mousePosition.y + 18f, 280f, 180f);
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label(item.DisplayName);
            GUILayout.Label("Type: " + item.Category);
            if (quantity > 1)
            {
                GUILayout.Label("Stack: " + quantity + " / " + item.StackLimit);
            }
            if (!string.IsNullOrEmpty(item.Description))
            {
                GUILayout.Label(item.Description);
            }
            GUILayout.Label("Tier: " + item.ResourceTier);
            if (!string.IsNullOrEmpty(item.DroppedBy))
            {
                GUILayout.Label("Dropped by: " + item.DroppedBy);
            }
            if (!string.IsNullOrEmpty(item.CraftingUsage))
            {
                GUILayout.Label("Used for: " + item.CraftingUsage);
            }

            EquipmentDefinition equipmentItem = item as EquipmentDefinition;
            if (equipmentItem != null)
            {
                GUILayout.Label("Slot: " + equipmentItem.Slot);
                GUILayout.Label("Rarity: " + equipmentItem.Rarity);
                GUILayout.Label("Level: " + equipmentItem.LevelRequirement);
                GUILayout.Label("Stats: ATK " + equipmentItem.Stats.Attack + " / DEF " + equipmentItem.Stats.Defense);
                GUILayout.Label("Visual: " + equipmentItem.VisualLayer);
            }

            ConsumableDefinition consumable = item as ConsumableDefinition;
            if (consumable != null)
            {
                GUILayout.Label("Effect: " + consumable.EffectDescription);
                GUILayout.Label("Duration: " + consumable.DurationSeconds + "s");
                GUILayout.Label("Cooldown: " + consumable.CooldownSeconds + "s");
            }

            CraftingPortDefinition port = item as CraftingPortDefinition;
            if (port != null)
            {
                GUILayout.Label("Crafting: " + port.StationType);
                GUILayout.Label("Speed: x" + port.SpeedMultiplier);
                if (!string.IsNullOrEmpty(port.FuelRules))
                {
                    GUILayout.Label("Fuel: " + port.FuelRules);
                }
            }
            GUILayout.EndArea();
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
