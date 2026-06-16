using ProjectEclipse.Combat;
using ProjectEclipse.Crafting;
using ProjectEclipse.Equipment;
using ProjectEclipse.Furnace;
using ProjectEclipse.Inventory;
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
        private InventoryPanel inventoryPanel;
        private CraftingPanel craftingPanel;
        private CraftingPortPanel craftingPortPanel;
        private bool inventoryOpen;

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

            inventoryPanel = new InventoryPanel(inventory, equipment, inventoryCrafting, furnace);
            craftingPanel = new CraftingPanel(crafting);
            craftingPortPanel = new CraftingPortPanel(inventoryCrafting, furnace);
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

            if (inventoryPanel == null || craftingPanel == null || craftingPortPanel == null)
            {
                GUILayout.Window(2, new Rect(12f, 140f, 300f, 90f), id =>
                {
                    GUILayout.Label("HUD systems not initialized.");
                    GUI.DragWindow();
                }, "Inventory");
                return;
            }

            ItemHoverState hover = new ItemHoverState();
            GUILayout.Window(2, new Rect(12f, 140f, 500f, 390f), id => inventoryPanel.Draw(id, hover), "Inventory");
            GUILayout.Window(3, new Rect(Screen.width - 348f, 12f, 336f, 240f), id => craftingPanel.Draw(id), "Crafting");
            GUILayout.Window(4, new Rect(Screen.width - 348f, 260f, 336f, 210f), id => craftingPortPanel.Draw(id), "Crafting Ports");

            if (hover.Item != null)
            {
                ItemTooltipView.Draw(hover.Item, hover.Quantity);
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
            GUILayout.Label("Mainhand: " + weaponName);
            string offhandName = equipment != null && equipment.Offhand != null ? equipment.Offhand.DisplayName : "None";
            GUILayout.Label("Offhand: " + offhandName);
            GUI.DragWindow();
        }
    }
}
