using System.Collections.Generic;
using ProjectEclipse.Combat;
using ProjectEclipse.Crafting;
using ProjectEclipse.Enemies;
using ProjectEclipse.Equipment;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using ProjectEclipse.Player;
using ProjectEclipse.UI;
using UnityEngine;

namespace ProjectEclipse.World
{
    public class MvpGameManager : MonoBehaviour
    {
        [System.Serializable]
        private class DebugInventorySeed
        {
            [SerializeField] private ItemDefinition item;
            [SerializeField] private int quantity = 1;

            public ItemDefinition Item { get { return item; } }
            public int Quantity { get { return Mathf.Max(1, quantity); } }
        }

#pragma warning disable CS0649
        [Header("Player")]
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerClassDefinition playerClass;
        [SerializeField] private Health playerHealth;
        [SerializeField] private PlayerResource playerResource;
        [SerializeField] private PlayerRespawnController playerRespawn;
        [SerializeField] private InventoryStore playerInventory;
        [SerializeField] private CombatController playerCombat;
        [SerializeField] private CombatInputRouter playerCombatInput;
        [SerializeField] private WarriorSkillController warriorSkillController;
        [SerializeField] private EquipmentController playerEquipment;
        [SerializeField] private CraftingSystem playerCrafting;
        [SerializeField] private WeaponDefinition starterWeapon;

        [Header("World Systems")]
        [SerializeField] private DropSpawner dropSpawner;
        [SerializeField] private MvpHud hud;
        [SerializeField] private MvpRoomFlowBuilder roomFlowBuilder;
        [SerializeField] private EnemySpawnManager enemySpawnManager;

        [Header("Scene Content")]
        [SerializeField] private List<EnemyController> placedEnemies = new List<EnemyController>();
        [SerializeField] private List<EnemyDefinition> routeEnemyDefinitions = new List<EnemyDefinition>();
        [SerializeField] private List<CraftingRecipe> availableRecipes = new List<CraftingRecipe>();

        [Header("Debug/Test")]
        [SerializeField] private bool debugSeedCopperSwordTestKit;
        [SerializeField] private List<DebugInventorySeed> debugInventorySeeds = new List<DebugInventorySeed>();
#pragma warning restore CS0649

        private void Awake()
        {
            Physics2D.gravity = new Vector2(0f, -18f);
            ResolveMissingReferences();
            WireRoomFlow();
            WirePlayer();
            ApplyDebugInventorySeeds();
            WireCrafting();
            WireEnemies();
            WireHud();
        }

        private void ResolveMissingReferences()
        {
            if (player == null)
            {
                player = FindAnyObjectByType<PlayerController>();
            }

            if (player != null)
            {
                if (playerHealth == null)
                {
                    playerHealth = player.GetComponent<Health>();
                }

                if (playerInventory == null)
                {
                    playerInventory = player.GetComponent<InventoryStore>();
                }

                if (playerResource == null)
                {
                    playerResource = player.GetComponent<PlayerResource>();
                }

                if (playerRespawn == null)
                {
                    playerRespawn = player.GetComponent<PlayerRespawnController>();
                }

                if (playerCombat == null)
                {
                    playerCombat = player.GetComponent<CombatController>();
                }

                if (playerCombatInput == null)
                {
                    playerCombatInput = player.GetComponent<CombatInputRouter>();
                }

                if (warriorSkillController == null)
                {
                    warriorSkillController = player.GetComponent<WarriorSkillController>();
                }

                if (playerEquipment == null)
                {
                    playerEquipment = player.GetComponent<EquipmentController>();
                }

                if (playerCrafting == null)
                {
                    playerCrafting = player.GetComponent<CraftingSystem>();
                }
            }

            if (dropSpawner == null)
            {
                dropSpawner = FindAnyObjectByType<DropSpawner>();
            }

            if (hud == null)
            {
                hud = FindAnyObjectByType<MvpHud>();
            }

            if (roomFlowBuilder == null)
            {
                roomFlowBuilder = FindAnyObjectByType<MvpRoomFlowBuilder>();
            }

            if (enemySpawnManager == null)
            {
                enemySpawnManager = FindAnyObjectByType<EnemySpawnManager>();
            }
        }

        private void WireRoomFlow()
        {
            if (roomFlowBuilder == null)
            {
                roomFlowBuilder = gameObject.AddComponent<MvpRoomFlowBuilder>();
            }

            roomFlowBuilder.ConfigureEnemyDefinitions(GetRouteEnemyDefinitions());
            roomFlowBuilder.Initialize(player);
        }

        private IEnumerable<EnemyDefinition> GetRouteEnemyDefinitions()
        {
            for (int i = 0; i < routeEnemyDefinitions.Count; i++)
            {
                if (routeEnemyDefinitions[i] != null)
                {
                    yield return routeEnemyDefinitions[i];
                }
            }

            for (int i = 0; i < placedEnemies.Count; i++)
            {
                EnemyController enemy = placedEnemies[i];
                if (enemy != null && enemy.Definition != null)
                {
                    yield return enemy.Definition;
                }
            }
        }

        private void WirePlayer()
        {
            if (playerClass != null && playerHealth != null)
            {
                playerHealth.SetMaxHealth(playerClass.StartingMaxHealth, true);
            }

            if (player != null && playerResource == null)
            {
                playerResource = player.gameObject.AddComponent<PlayerResource>();
            }

            if (player != null && playerCombatInput == null)
            {
                playerCombatInput = player.gameObject.AddComponent<CombatInputRouter>();
            }

            if (player != null && warriorSkillController == null)
            {
                warriorSkillController = player.gameObject.AddComponent<WarriorSkillController>();
            }

            if (player != null && playerRespawn == null)
            {
                playerRespawn = player.gameObject.AddComponent<PlayerRespawnController>();
            }

            if (playerRespawn != null)
            {
                playerRespawn.Initialize(playerHealth, playerResource, roomFlowBuilder);
            }

            WeaponDefinition weaponToEquip = starterWeapon;
            if (weaponToEquip == null && playerClass != null)
            {
                weaponToEquip = playerClass.StartingWeapon;
            }

            if (playerEquipment != null)
            {
                playerEquipment.Initialize(playerCombat, playerInventory);
            }

            if (playerEquipment != null && weaponToEquip != null)
            {
                playerEquipment.TryEquipWeapon(weaponToEquip);
            }

            if (playerInventory != null && weaponToEquip != null)
            {
                while (playerInventory.HasItem(weaponToEquip, 1))
                {
                    playerInventory.RemoveItem(weaponToEquip, 1);
                }
            }
        }

        private void WireCrafting()
        {
            if (playerCrafting != null)
            {
                playerCrafting.Initialize(playerInventory, playerEquipment, availableRecipes);
            }
        }

        private void ApplyDebugInventorySeeds()
        {
            if (!debugSeedCopperSwordTestKit || playerInventory == null)
            {
                return;
            }

            for (int i = 0; i < debugInventorySeeds.Count; i++)
            {
                DebugInventorySeed seed = debugInventorySeeds[i];
                if (seed == null || seed.Item == null)
                {
                    continue;
                }

                playerInventory.AddItem(seed.Item, seed.Quantity);
            }

            Debug.Log("Debug/Test Copper Sword kit seeded into player inventory.");
        }

        private void WireEnemies()
        {
            Transform playerTransform = player != null ? player.transform : null;
            if (enemySpawnManager == null)
            {
                enemySpawnManager = gameObject.AddComponent<EnemySpawnManager>();
            }

            enemySpawnManager.Initialize(placedEnemies, playerTransform, dropSpawner);
        }

        private void WireHud()
        {
            if (hud != null)
            {
                hud.Initialize(playerHealth, playerResource, playerInventory, playerEquipment, playerCrafting, dropSpawner);
                hud.SetRespawnController(playerRespawn);
            }
        }
    }
}
