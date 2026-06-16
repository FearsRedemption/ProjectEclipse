using System.Collections.Generic;
using ProjectEclipse.Combat;
using ProjectEclipse.Crafting;
using ProjectEclipse.Enemies;
using ProjectEclipse.Equipment;
using ProjectEclipse.Furnace;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using ProjectEclipse.Player;
using ProjectEclipse.Progression;
using ProjectEclipse.UI;
using ProjectEclipse.Utilities;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectEclipse.World
{
    public class PrototypeBootstrapper : MonoBehaviour
    {
        private RuntimeCatalog catalog;
        private Transform playerTransform;
        private DropSpawner dropSpawner;

        private void Awake()
        {
            Physics2D.gravity = new Vector2(0f, -24f);
            catalog = BuildCatalog();
            dropSpawner = gameObject.AddComponent<DropSpawner>();

            CreateCamera();
            CreateWorld();
            PlayerRuntime player = CreatePlayer();
            playerTransform = player.Root.transform;
            CreateFurnaceStation(player.Inventory, out FurnaceSystem furnaceSystem);
            player.Crafting.Initialize(player.Inventory, player.Equipment, catalog.Recipes);
            CreateEnemies();
            CreateHud(player.Health, player.Inventory, player.Equipment, player.Crafting, furnaceSystem);
        }

        private void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.backgroundColor = new Color(0.08f, 0.1f, 0.13f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<CameraFollow2D>();
            cameraObject.transform.position = new Vector3(0f, 1.5f, -10f);
        }

        private void CreateWorld()
        {
            CreateSolid("Ground", new Vector3(0f, -2.2f, 0f), new Vector2(34f, 1f), new Color(0.23f, 0.2f, 0.16f), true, -3);
            CreateSolid("Low Platform", new Vector3(-5.5f, 0f, 0f), new Vector2(5.5f, 0.45f), new Color(0.28f, 0.25f, 0.2f), true, -3);
            CreateSolid("Coal Ridge", new Vector3(7.5f, -0.25f, 0f), new Vector2(5f, 0.45f), new Color(0.18f, 0.18f, 0.2f), true, -3);
            CreateSolid("Copper Shelf", new Vector3(13.2f, 0.8f, 0f), new Vector2(4.4f, 0.45f), new Color(0.33f, 0.19f, 0.12f), true, -3);

            CreateSolid("Forest Zone Tint", new Vector3(-7f, 1.3f, 0.5f), new Vector2(8f, 7f), new Color(0.08f, 0.16f, 0.11f, 0.35f), false, -8);
            CreateSolid("Stone Zone Tint", new Vector3(1.2f, 1.3f, 0.5f), new Vector2(8f, 7f), new Color(0.16f, 0.16f, 0.17f, 0.35f), false, -8);
            CreateSolid("Coal Zone Tint", new Vector3(8.8f, 1.3f, 0.5f), new Vector2(7.5f, 7f), new Color(0.08f, 0.08f, 0.1f, 0.35f), false, -8);
            CreateSolid("Copper Zone Tint", new Vector3(15.2f, 1.3f, 0.5f), new Vector2(6.5f, 7f), new Color(0.26f, 0.13f, 0.06f, 0.35f), false, -8);
        }

        private PlayerRuntime CreatePlayer()
        {
            GameObject player = CreateActor("Player", new Vector3(-12.5f, -0.8f, 0f), new Vector2(0.8f, 1.25f), new Color(0.28f, 0.75f, 1f), 10);
            TryAttachAnimator(player, "Player");

            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 3f;
            body.freezeRotation = true;

            BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;

            VisualStateAnimator visual = player.AddComponent<VisualStateAnimator>();
            visual.SetBaseColor(new Color(0.28f, 0.75f, 1f));

            Health health = player.AddComponent<Health>();
            health.SetMaxHealth(24, true);
            health.Died += delegate
            {
                Debug.Log("Project Eclipse: player defeated. Reload the MVP scene to retry.");
            };

            InventoryStore inventory = player.AddComponent<InventoryStore>();
            CombatController combat = player.AddComponent<CombatController>();
            EquipmentController equipment = player.AddComponent<EquipmentController>();
            equipment.Initialize(combat, inventory);
            PlayerController controller = player.AddComponent<PlayerController>();
            CraftingSystem crafting = player.AddComponent<CraftingSystem>();

            inventory.AddItem(catalog.StarterWeapon, 1);
            equipment.TryEquipWeapon(catalog.StarterWeapon);

            CameraFollow2D follow = Camera.main != null ? Camera.main.GetComponent<CameraFollow2D>() : null;
            if (follow != null)
            {
                follow.SetTarget(player.transform);
            }

            return new PlayerRuntime(player, health, inventory, equipment, crafting, controller);
        }

        private void CreateFurnaceStation(InventoryStore inventory, out FurnaceSystem furnaceSystem)
        {
            GameObject furnace = CreateSolid("Basic Furnace Station", new Vector3(-9.8f, -1.35f, 0f), new Vector2(0.9f, 0.9f), new Color(0.65f, 0.28f, 0.16f), true, 3);
            furnaceSystem = furnace.AddComponent<FurnaceSystem>();
            furnaceSystem.Initialize(inventory);
        }

        private void CreateEnemies()
        {
            CreateEnemy(catalog.TreeCreature, new Vector3(-6.6f, -0.8f, 0f));
            CreateEnemy(catalog.TreeCreature, new Vector3(-4.7f, 0.8f, 0f));
            CreateEnemy(catalog.StoneCreature, new Vector3(0.5f, -0.8f, 0f));
            CreateEnemy(catalog.StoneCreature, new Vector3(3.3f, -0.8f, 0f));
            CreateEnemy(catalog.CoalCreature, new Vector3(7.8f, 0.7f, 0f));
            CreateEnemy(catalog.CopperCreature, new Vector3(13.3f, 1.7f, 0f));
            CreateEnemy(catalog.CopperCreature, new Vector3(16.5f, -0.8f, 0f));
        }

        private void CreateEnemy(EnemyDefinition definition, Vector3 position)
        {
            GameObject enemy = CreateActor(definition.DisplayName, position, definition.VisualScale, definition.PlaceholderColor, 8);
            TryAttachAnimator(enemy, "Enemy");

            Rigidbody2D body = enemy.AddComponent<Rigidbody2D>();
            body.gravityScale = 3f;
            body.freezeRotation = true;

            BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
            collider.size = definition.ColliderSize;

            SpriteSheetAnimator sheetAnimator = enemy.AddComponent<SpriteSheetAnimator>();
            VisualStateAnimator visual = enemy.AddComponent<VisualStateAnimator>();
            Texture2D sheet = LoadTextureAtPath(definition.SpriteSheetPath);
            SpriteRenderer renderer = enemy.GetComponent<SpriteRenderer>();
            if (sheet != null)
            {
                if (renderer != null)
                {
                    renderer.color = Color.white;
                }

                sheetAnimator.Configure(sheet, 96, 96, 96f);
                visual.SetBaseColor(Color.white);
            }
            else
            {
                visual.SetBaseColor(definition.PlaceholderColor);
            }

            EnemyController controller = enemy.AddComponent<EnemyController>();
            controller.Initialize(definition, playerTransform, dropSpawner);
        }

        private void CreateHud(
            Health health,
            InventoryStore inventory,
            EquipmentController equipment,
            CraftingSystem crafting,
            FurnaceSystem furnaceSystem)
        {
            GameObject hudObject = new GameObject("MVP HUD");
            MvpHud hud = hudObject.AddComponent<MvpHud>();
            hud.Initialize(health, inventory, equipment, crafting, furnaceSystem);
        }

        private GameObject CreateActor(string name, Vector3 position, Vector2 scale, Color color, int sortingOrder)
        {
            GameObject actor = new GameObject(name);
            actor.transform.position = position;
            actor.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            SpriteRenderer renderer = actor.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.GetSquareSprite(color);
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return actor;
        }

        private GameObject CreateSolid(string name, Vector3 position, Vector2 scale, Color color, bool colliderEnabled, int sortingOrder)
        {
            GameObject solid = new GameObject(name);
            solid.transform.position = position;
            solid.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            SpriteRenderer renderer = solid.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.GetSquareSprite(color);
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            if (colliderEnabled)
            {
                BoxCollider2D collider = solid.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;
            }

            return solid;
        }

        private RuntimeCatalog BuildCatalog()
        {
            RuntimeCatalog result = new RuntimeCatalog();

            result.Wood = CreateItem("wood", "Tree Material", ItemCategory.Material, new Color(0.38f, 0.24f, 0.12f), "Assets/ProjectEclipse/Art/Items/wood_icon.png");
            result.Stone = CreateItem("stone", "Stone", ItemCategory.Material, new Color(0.55f, 0.57f, 0.58f), "Assets/ProjectEclipse/Art/Items/stone_icon.png");
            result.Coal = CreateItem("coal", "Coal", ItemCategory.Material, new Color(0.08f, 0.08f, 0.09f), "Assets/ProjectEclipse/Art/Items/coal_icon.png");
            result.Copper = CreateItem("copper_fragments", "Copper Fragments", ItemCategory.Material, new Color(0.78f, 0.35f, 0.12f), "Assets/ProjectEclipse/Art/Items/copper_icon.png");
            result.BasicFurnace = CreateItem("basic_furnace", "Basic Furnace", ItemCategory.Furnace, new Color(0.66f, 0.28f, 0.16f));
            result.CopperWhetstone = CreateItem("copper_whetstone", "Copper Whetstone Placeholder", ItemCategory.Upgrade, new Color(0.95f, 0.45f, 0.18f));

            result.StarterWeapon = CreateWeapon("starter_blade", "Starter Blade", WeaponArchetype.StarterMelee, 2, 1.35f, 1.05f, 0.36f, 3.1f, new Color(0.35f, 0.72f, 1f));
            result.StoneWeapon = CreateWeapon("stone_cleaver", "Stone Cleaver", WeaponArchetype.HeavyHammer, 5, 1.55f, 1.15f, 0.55f, 4.5f, new Color(0.58f, 0.6f, 0.62f));

            result.TreeCreature = CreateEnemyDefinition(
                "tree_creature",
                "Tree Creature",
                ResourceTier.Wood,
                5,
                1,
                0.95f,
                5.2f,
                0.85f,
                1.35f,
                0.45f,
                2.2f,
                new Vector2(0.82f, 0.82f),
                new Vector2(0.72f, 0.72f),
                "Assets/ProjectEclipse/Art/Creatures/tree_creature_sheet.png",
                new Color(0.24f, 0.62f, 0.24f),
                new DropTableEntry(result.Wood, 2, 5, 1f));

            result.StoneCreature = CreateEnemyDefinition(
                "stone_creature",
                "Stone Creature",
                ResourceTier.Stone,
                12,
                2,
                0.75f,
                5.8f,
                0.9f,
                1.35f,
                0.8f,
                4.2f,
                new Vector2(1.05f, 1.05f),
                new Vector2(0.9f, 0.82f),
                "Assets/ProjectEclipse/Art/Creatures/stone_creature_sheet.png",
                new Color(0.5f, 0.52f, 0.55f),
                new DropTableEntry(result.Stone, 2, 4, 1f));

            result.CoalCreature = CreateEnemyDefinition(
                "coal_creature",
                "Coal Creature",
                ResourceTier.Coal,
                11,
                2,
                2.05f,
                6.2f,
                0.9f,
                0.65f,
                2.25f,
                2.8f,
                new Vector2(0.9f, 0.9f),
                new Vector2(0.72f, 0.78f),
                "Assets/ProjectEclipse/Art/Creatures/coal_creature_sheet.png",
                new Color(0.1f, 0.11f, 0.13f),
                new DropTableEntry(result.Coal, 1, 3, 1f),
                new DropTableEntry(result.Stone, 1, 2, 0.35f));

            result.CopperCreature = CreateEnemyDefinition(
                "copper_creature",
                "Copper Creature",
                ResourceTier.Copper,
                20,
                4,
                1.35f,
                6.6f,
                1.15f,
                0.9f,
                4.2f,
                5.2f,
                new Vector2(1.22f, 1.22f),
                new Vector2(1.02f, 0.9f),
                "Assets/ProjectEclipse/Art/Creatures/copper_creature_sheet.png",
                new Color(0.76f, 0.32f, 0.12f),
                new DropTableEntry(result.Copper, 1, 3, 1f),
                new DropTableEntry(result.Coal, 1, 1, 0.45f));

            result.Recipes.Add(CreateRecipe(
                "stone_weapon",
                "Craft Stone Cleaver",
                result.StoneWeapon,
                1,
                true,
                new CraftingIngredient(result.Stone, 10)));

            result.Recipes.Add(CreateRecipe(
                "basic_furnace",
                "Craft Basic Furnace",
                result.BasicFurnace,
                1,
                false,
                new CraftingIngredient(result.Stone, 12),
                new CraftingIngredient(result.Coal, 3)));

            result.Recipes.Add(CreateRecipe(
                "copper_whetstone_placeholder",
                "Craft Copper Whetstone Placeholder",
                result.CopperWhetstone,
                1,
                false,
                new CraftingIngredient(result.Copper, 8),
                new CraftingIngredient(result.Coal, 2)));

            result.Dimensions.Add(CreateTier("earth_forest", "Earth / Forest", string.Empty, ResourceTier.Wood, "Forest God Placeholder", new[] { "Bark Brute", "Root Witch" }, result.TreeCreature));
            result.Dimensions.Add(CreateTier("stone_tier", "Earth / Stone Tier", "forest_god_placeholder", ResourceTier.Stone, "Stone Warden Placeholder", new[] { "Pebble Knight", "Granite Maw" }, result.TreeCreature, result.StoneCreature));
            result.Dimensions.Add(CreateTier("coal_tier", "Earth / Coal Tier", "stone_warden_placeholder", ResourceTier.Coal, "Ash Furnace God Placeholder", new[] { "Soot Runner", "Charcoal Hulk" }, result.StoneCreature, result.CoalCreature));
            result.Dimensions.Add(CreateTier("copper_tier", "Earth / Copper Tier", "ash_furnace_god_placeholder", ResourceTier.Copper, "Copper God Placeholder", new[] { "Wire Imp", "Oxide Knight" }, result.CoalCreature, result.CopperCreature));

            return result;
        }

        private static ItemDefinition CreateItem(string id, string displayName, ItemCategory category, Color color, string iconPath = null)
        {
            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
            Sprite icon = LoadSpriteAtPath(iconPath, 64f);
            item.Configure(id, displayName, category, color, 999, icon != null ? icon : SpriteFactory.GetSquareSprite(color));
            return item;
        }

        private static WeaponDefinition CreateWeapon(
            string id,
            string displayName,
            WeaponArchetype archetype,
            int damage,
            float range,
            float height,
            float cooldown,
            float knockback,
            Color color)
        {
            WeaponDefinition weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ConfigureWeapon(id, displayName, archetype, damage, range, height, cooldown, knockback, color, SpriteFactory.GetSquareSprite(color));
            return weapon;
        }

        private static EnemyDefinition CreateEnemyDefinition(
            string id,
            string displayName,
            ResourceTier tier,
            int health,
            int damage,
            float speed,
            float detectionRange,
            float attackRange,
            float cooldown,
            float lungeForce,
            float knockback,
            Vector2 scale,
            Vector2 colliderSize,
            string spriteSheetPath,
            Color color,
            params DropTableEntry[] drops)
        {
            EnemyDefinition enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            enemy.Configure(id, displayName, tier, health, damage, speed, detectionRange, attackRange, cooldown, lungeForce, knockback, scale, colliderSize, spriteSheetPath, color, drops);
            return enemy;
        }

        private static CraftingRecipe CreateRecipe(
            string id,
            string displayName,
            ItemDefinition output,
            int outputQuantity,
            bool autoEquip,
            params CraftingIngredient[] ingredients)
        {
            CraftingRecipe recipe = ScriptableObject.CreateInstance<CraftingRecipe>();
            recipe.Configure(id, displayName, ingredients, output, outputQuantity, autoEquip);
            return recipe;
        }

        private static DimensionTierDefinition CreateTier(
            string id,
            string displayName,
            string requiredBossId,
            ResourceTier tier,
            string mainBoss,
            IEnumerable<string> miniBosses,
            params EnemyDefinition[] enemies)
        {
            DimensionTierDefinition definition = ScriptableObject.CreateInstance<DimensionTierDefinition>();
            definition.Configure(id, displayName, requiredBossId, tier, enemies, mainBoss, miniBosses);
            return definition;
        }

        private static void TryAttachAnimator(GameObject target, string controllerName)
        {
#if UNITY_EDITOR
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/ProjectEclipse/Animations/" + controllerName + ".controller");
            if (controller != null)
            {
                Animator animator = target.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
            }
#endif
        }

        private static Texture2D LoadTextureAtPath(string assetPath)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
#else
            return null;
#endif
        }

        private static Sprite LoadSpriteAtPath(string assetPath, float pixelsPerUnit)
        {
            Texture2D texture = LoadTextureAtPath(assetPath);
            if (texture == null)
            {
                return null;
            }

            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        private sealed class PlayerRuntime
        {
            public readonly GameObject Root;
            public readonly Health Health;
            public readonly InventoryStore Inventory;
            public readonly EquipmentController Equipment;
            public readonly CraftingSystem Crafting;
            public readonly PlayerController Controller;

            public PlayerRuntime(
                GameObject root,
                Health health,
                InventoryStore inventory,
                EquipmentController equipment,
                CraftingSystem crafting,
                PlayerController controller)
            {
                Root = root;
                Health = health;
                Inventory = inventory;
                Equipment = equipment;
                Crafting = crafting;
                Controller = controller;
            }
        }

        private sealed class RuntimeCatalog
        {
            public ItemDefinition Wood;
            public ItemDefinition Stone;
            public ItemDefinition Coal;
            public ItemDefinition Copper;
            public ItemDefinition BasicFurnace;
            public ItemDefinition CopperWhetstone;
            public WeaponDefinition StarterWeapon;
            public WeaponDefinition StoneWeapon;
            public EnemyDefinition TreeCreature;
            public EnemyDefinition StoneCreature;
            public EnemyDefinition CoalCreature;
            public EnemyDefinition CopperCreature;
            public readonly List<CraftingRecipe> Recipes = new List<CraftingRecipe>();
            public readonly List<DimensionTierDefinition> Dimensions = new List<DimensionTierDefinition>();
        }
    }
}
