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
        private CombatInputRouter combatInput;
        private CraftingSystem crafting;
        private InventoryCraftingController inventoryCrafting;
        private FurnaceSystem furnace;
        private InventoryScreen inventoryScreen;
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
            combatInput = playerEquipment != null ? playerEquipment.GetComponent<CombatInputRouter>() : null;
            if (combatInput == null && health != null)
            {
                combatInput = health.GetComponent<CombatInputRouter>();
            }
            crafting = craftingSystem;
            inventoryCrafting = store != null ? store.GetComponent<InventoryCraftingController>() : null;
            furnace = furnaceSystem;

            inventoryScreen = new InventoryScreen(inventory, equipment, inventoryCrafting, crafting, furnace);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                inventoryOpen = !inventoryOpen;
                if (!inventoryOpen && inventoryScreen != null)
                {
                    inventoryScreen.ResetCraftingTransientState();
                }
            }
        }

        private void OnGUI()
        {
            GameGuiStyles.ApplySkin(GUI.skin);

            GUILayout.Window(1, new Rect(12f, 12f, 280f, 126f), DrawStatusWindow, "Status", GameGuiStyles.Window);
            DrawCombatFeedback();
            DrawWorkOrderTracker();
            if (!inventoryOpen)
            {
                return;
            }

            if (inventoryScreen == null)
            {
                GUILayout.Window(2, new Rect(12f, 140f, 300f, 90f), id =>
                {
                    GUILayout.Label("HUD systems not initialized.");
                    GUI.DragWindow();
                }, "Inventory");
                return;
            }

            ItemHoverState hover = new ItemHoverState();
            float width = Mathf.Max(720f, Mathf.Min(1040f, Screen.width - 24f));
            float height = Mathf.Max(420f, Mathf.Min(650f, Screen.height - 152f));
            GUILayout.Window(2, new Rect(12f, 146f, width, height), id => inventoryScreen.Draw(id, hover), "Inventory / Equipment / Crafting", GameGuiStyles.Window);

            if (hover.HasHover)
            {
                ItemTooltipView.Draw(hover, equipment, inventoryCrafting);
            }
        }

        private void DrawWorkOrderTracker()
        {
            if (crafting == null || crafting.ActiveWorkOrder == null)
            {
                return;
            }

            float width = 360f;
            float x = Mathf.Max(304f, Screen.width - width - 12f);
            GUILayout.Window(5, new Rect(x, 12f, width, 300f), id =>
            {
                WorkOrderTrackerPanel.Draw(crafting);
                GUI.DragWindow();
            }, "Work Order", GameGuiStyles.Window);
        }

        private void DrawStatusWindow(int windowId)
        {
            if (playerHealth != null)
            {
                GUILayout.Label("Health: " + playerHealth.CurrentHealth + " / " + playerHealth.MaxHealth);
                Rect bar = GUILayoutUtility.GetRect(240f, 16f);
                float fill = playerHealth.MaxHealth > 0 ? (float)playerHealth.CurrentHealth / playerHealth.MaxHealth : 0f;
                GameGuiStyles.DrawProgressBar(bar, fill, new Color(0.8f, 0.18f, 0.14f, 1f));
            }

            string weaponName = equipment != null && equipment.EquippedWeapon != null ? equipment.EquippedWeapon.DisplayName : "None";
            GUILayout.Label("Mainhand: " + weaponName);
            string offhandName = equipment != null && equipment.Offhand != null ? equipment.Offhand.DisplayName : "None";
            GUILayout.Label("Offhand: " + offhandName);
            GUI.DragWindow();
        }

        private void DrawCombatFeedback()
        {
            if (combatInput == null || !combatInput.HasFeedback)
            {
                return;
            }

            float width = 360f;
            Rect rect = new Rect((Screen.width - width) * 0.5f, 78f, width, 32f);
            GUI.Label(rect, combatInput.FeedbackText, GameGuiStyles.FeedbackLabel);
        }
    }
}
