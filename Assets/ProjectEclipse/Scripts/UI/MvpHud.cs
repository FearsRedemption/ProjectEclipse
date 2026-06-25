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
        private PlayerResource playerResource;
        private InventoryStore inventory;
        private EquipmentController equipment;
        private CombatInputRouter combatInput;
        private CraftingSystem crafting;
        private InventoryCraftingController inventoryCrafting;
        private FurnaceSystem furnace;
        private DropSpawner dropSpawner;
        private InventoryScreen inventoryScreen;
        private bool inventoryOpen;
        private WorkOrder trackedWorkOrder;
        private float workOrderCompletedAt = -1f;
        private Rect inventoryWindowRect;

        [SerializeField] private float workOrderCompletionHoldSeconds = 2f;
        [SerializeField] private float workOrderCompletionFadeSeconds = 1.4f;

        public static bool PointerBlocksGameplayInput { get; private set; }

        public void Initialize(
            Health health,
            PlayerResource resource,
            InventoryStore store,
            EquipmentController playerEquipment,
            CraftingSystem craftingSystem,
            FurnaceSystem furnaceSystem,
            DropSpawner worldDropSpawner = null)
        {
            playerHealth = health;
            playerResource = resource;
            inventory = store;
            equipment = playerEquipment;
            dropSpawner = worldDropSpawner != null ? worldDropSpawner : FindAnyObjectByType<DropSpawner>();
            combatInput = playerEquipment != null ? playerEquipment.GetComponent<CombatInputRouter>() : null;
            if (combatInput == null && health != null)
            {
                combatInput = health.GetComponent<CombatInputRouter>();
            }
            crafting = craftingSystem;
            inventoryCrafting = store != null ? store.GetComponent<InventoryCraftingController>() : null;
            furnace = furnaceSystem;

            inventoryScreen = new InventoryScreen(inventory, equipment, inventoryCrafting, crafting, furnace, dropSpawner);
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

            GUILayout.Window(1, new Rect(12f, 12f, 280f, 150f), DrawStatusWindow, "Status", GameGuiStyles.Window);
            DrawCombatFeedback();
            DrawWorkOrderTracker();
            if (!inventoryOpen)
            {
                PointerBlocksGameplayInput = false;
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
            inventoryWindowRect = GUILayout.Window(2, new Rect(12f, 146f, width, height), id => inventoryScreen.Draw(id, hover), "Inventory / Equipment / Crafting", GameGuiStyles.Window);
            PointerBlocksGameplayInput = inventoryOpen && inventoryWindowRect.Contains(GetPointerGuiPosition());
            inventoryScreen.HandlePendingDragDrop(inventoryWindowRect);

            if (hover.HasHover)
            {
                ItemTooltipView.Draw(hover, equipment, inventoryCrafting);
            }
        }

        private static Vector2 GetPointerGuiPosition()
        {
            Vector3 mouse = Input.mousePosition;
            return new Vector2(mouse.x, Screen.height - mouse.y);
        }

        private void DrawWorkOrderTracker()
        {
            if (crafting == null || crafting.ActiveWorkOrder == null)
            {
                trackedWorkOrder = null;
                workOrderCompletedAt = -1f;
                return;
            }

            WorkOrder order = crafting.ActiveWorkOrder;
            if (trackedWorkOrder != order)
            {
                trackedWorkOrder = order;
                workOrderCompletedAt = -1f;
            }

            float alpha = 1f;
            if (order.IsComplete)
            {
                if (workOrderCompletedAt < 0f)
                {
                    workOrderCompletedAt = Time.time;
                }

                float elapsed = Time.time - workOrderCompletedAt;
                float hold = Mathf.Max(0f, workOrderCompletionHoldSeconds);
                float fade = Mathf.Max(0.1f, workOrderCompletionFadeSeconds);
                if (elapsed >= hold + fade)
                {
                    crafting.ClearActiveWorkOrder();
                    trackedWorkOrder = null;
                    workOrderCompletedAt = -1f;
                    return;
                }

                if (elapsed > hold)
                {
                    alpha = 1f - Mathf.Clamp01((elapsed - hold) / fade);
                }
            }

            float width = 360f;
            float x = Mathf.Max(304f, Screen.width - width - 12f);
            Color oldColor = GUI.color;
            GUI.color = new Color(oldColor.r, oldColor.g, oldColor.b, oldColor.a * alpha);
            GUILayout.Window(5, new Rect(x, 12f, width, 300f), id =>
            {
                WorkOrderTrackerPanel.Draw(crafting, true, true);
                GUI.DragWindow();
            }, "Work Order", GameGuiStyles.Window);
            GUI.color = oldColor;
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

            if (playerResource != null)
            {
                GUILayout.Label("MP: " + playerResource.CurrentMp + " / " + playerResource.MaxMp);
                Rect mpBar = GUILayoutUtility.GetRect(240f, 14f);
                GameGuiStyles.DrawProgressBar(mpBar, playerResource.NormalizedMp, new Color(0.2f, 0.48f, 0.95f, 1f));
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
