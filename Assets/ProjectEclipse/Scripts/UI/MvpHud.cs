using ProjectEclipse.Combat;
using ProjectEclipse.Crafting;
using ProjectEclipse.Equipment;
using ProjectEclipse.Input;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using ProjectEclipse.Player;
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
        private DropSpawner dropSpawner;
        private PlayerRespawnController respawnController;
        private InventoryScreen inventoryScreen;
        private bool inventoryOpen;
        private bool initialized;
        private Rect inventoryWindowRect;

        public static bool PointerBlocksGameplayInput { get; private set; }

        public void Initialize(
            Health health,
            PlayerResource resource,
            InventoryStore store,
            EquipmentController playerEquipment,
            CraftingSystem craftingSystem,
            DropSpawner worldDropSpawner = null)
        {
            playerHealth = health;
            playerResource = resource;
            inventory = store;
            equipment = playerEquipment;
            dropSpawner = worldDropSpawner != null ? worldDropSpawner : FindAnyObjectByType<DropSpawner>();
            respawnController = health != null ? health.GetComponent<PlayerRespawnController>() : null;
            combatInput = playerEquipment != null ? playerEquipment.GetComponent<CombatInputRouter>() : null;
            if (combatInput == null && health != null)
            {
                combatInput = health.GetComponent<CombatInputRouter>();
            }
            crafting = craftingSystem;
            inventoryCrafting = store != null ? store.GetComponent<InventoryCraftingController>() : null;

            BuildInventoryScreen();
            initialized = true;
        }

        public void SetRespawnController(PlayerRespawnController playerRespawn)
        {
            respawnController = playerRespawn != null ? playerRespawn : respawnController;
        }

        private void Update()
        {
            EnsureInitialized();
            if (respawnController != null && respawnController.IsRespawning)
            {
                inventoryOpen = false;
                PointerBlocksGameplayInput = true;
                return;
            }

            if (GameInput.WasPressedThisFrame(GameInputKey.Tab))
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
            EnsureInitialized();

            GUILayout.Window(1, new Rect(12f, 12f, 280f, 150f), DrawStatusWindow, "Status", GameGuiStyles.Window);
            DrawCombatFeedback();
            DrawWorkOrderTracker();
            DrawDeathRespawnOverlay();
            if (respawnController != null && respawnController.IsRespawning)
            {
                inventoryOpen = false;
                PointerBlocksGameplayInput = true;
                return;
            }

            if (!inventoryOpen)
            {
                PointerBlocksGameplayInput = respawnController != null && respawnController.IsRespawning;
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
            Vector2 mouse = GameInput.PointerScreenPosition;
            return new Vector2(mouse.x, Screen.height - mouse.y);
        }

        private void EnsureInitialized()
        {
            if (initialized && inventoryScreen != null)
            {
                return;
            }

            if (playerHealth == null)
            {
                playerHealth = FindAnyObjectByType<Health>();
            }

            if (inventory == null)
            {
                inventory = FindAnyObjectByType<InventoryStore>();
            }

            if (playerResource == null && playerHealth != null)
            {
                playerResource = playerHealth.GetComponent<PlayerResource>();
            }
            if (playerResource == null && inventory != null)
            {
                playerResource = inventory.GetComponent<PlayerResource>();
            }

            if (equipment == null && inventory != null)
            {
                equipment = inventory.GetComponent<EquipmentController>();
            }

            if (crafting == null && inventory != null)
            {
                crafting = inventory.GetComponent<CraftingSystem>();
            }

            if (inventoryCrafting == null && inventory != null)
            {
                inventoryCrafting = inventory.GetComponent<InventoryCraftingController>();
            }

            if (dropSpawner == null)
            {
                dropSpawner = FindAnyObjectByType<DropSpawner>();
            }

            if (respawnController == null)
            {
                respawnController = playerHealth != null ? playerHealth.GetComponent<PlayerRespawnController>() : FindAnyObjectByType<PlayerRespawnController>();
            }

            if (combatInput == null)
            {
                combatInput = playerHealth != null ? playerHealth.GetComponent<CombatInputRouter>() : null;
            }

            if (inventory != null)
            {
                BuildInventoryScreen();
                initialized = true;
            }
        }

        private void BuildInventoryScreen()
        {
            inventoryScreen = new InventoryScreen(inventory, equipment, inventoryCrafting, crafting, dropSpawner);
        }

        private void DrawWorkOrderTracker()
        {
            if (crafting == null || crafting.QueuedWorkOrderCount == 0)
            {
                return;
            }

            float width = 360f;
            float x = Mathf.Max(304f, Screen.width - width - 12f);
            GUILayout.Window(5, new Rect(x, 12f, width, 420f), id =>
            {
                WorkOrderTrackerPanel.Draw(crafting, true, true);
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

        private void DrawDeathRespawnOverlay()
        {
            if (respawnController == null || !respawnController.IsRespawning)
            {
                return;
            }

            PointerBlocksGameplayInput = true;
            float width = Mathf.Min(420f, Screen.width - 48f);
            float height = 124f;
            Rect rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GameGuiStyles.DrawBox(rect, new Color(0.04f, 0.05f, 0.06f, 0.94f), new Color(0.55f, 0.08f, 0.08f, 1f), 2f);
            GUI.Label(new Rect(rect.x + 16f, rect.y + 22f, rect.width - 32f, 34f), "You Died", GameGuiStyles.HeaderLabel);
            int seconds = Mathf.CeilToInt(respawnController.RemainingSeconds);
            GUI.Label(new Rect(rect.x + 16f, rect.y + 62f, rect.width - 32f, 28f), "Respawning in " + seconds + "...", GameGuiStyles.CenterLabel);
        }
    }
}
