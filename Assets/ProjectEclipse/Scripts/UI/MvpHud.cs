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
        private FurnaceSystem furnace;
        private Vector2 inventoryScroll;

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
            furnace = furnaceSystem;
        }

        private void OnGUI()
        {
            GUI.skin.window.fontSize = 14;
            GUI.skin.label.fontSize = 13;
            GUI.skin.button.fontSize = 13;

            GUILayout.Window(1, new Rect(12f, 12f, 280f, 120f), DrawStatusWindow, "Status");
            GUILayout.Window(2, new Rect(12f, 140f, 300f, 300f), DrawStorageWindow, "Storage");
            GUILayout.Window(3, new Rect(Screen.width - 328f, 12f, 316f, 240f), DrawCraftingWindow, "Crafting");
            GUILayout.Window(4, new Rect(Screen.width - 328f, 260f, 316f, 170f), DrawFurnaceWindow, "Furnace");
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

        private void DrawStorageWindow(int windowId)
        {
            if (inventory == null)
            {
                GUILayout.Label("Storage missing.");
                return;
            }

            List<InventoryStack> stacks = inventory.GetSnapshot();
            inventoryScroll = GUILayout.BeginScrollView(inventoryScroll, GUILayout.Height(230f));
            for (int i = 0; i < stacks.Count; i++)
            {
                InventoryStack stack = stacks[i];
                if (stack.Item == null)
                {
                    continue;
                }

                GUI.backgroundColor = stack.Item.PlaceholderColor;
                if (GUILayout.Button(stack.Item.DisplayName + " x" + stack.Quantity))
                {
                    HandleStorageClick(stack.Item, stack.Quantity, Event.current != null && Event.current.shift);
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndScrollView();
            GUI.DragWindow();
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

