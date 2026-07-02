#if UNITY_EDITOR
using ProjectEclipse.Enemies;
using ProjectEclipse.Items;
using ProjectEclipse.Progression;
using UnityEditor;
using UnityEngine;

namespace ProjectEclipse.EditorTools
{
    [InitializeOnLoad]
    public static class RouteProgressionDataAuthoring
    {
        private const string ItemsFolder = "Assets/ProjectEclipse/Data/Items";
        private const string EnemiesFolder = "Assets/ProjectEclipse/Data/Enemies";
        private const string DropTablesFolder = "Assets/ProjectEclipse/Data/DropTables";
        private const string CreatureArtFolder = "Assets/ProjectEclipse/Art/Creatures/Route";
        private const string ItemArtFolder = "Assets/ProjectEclipse/Art/Items";

        private static bool running;

        private struct ItemSpec
        {
            public string Path;
            public string Id;
            public string DisplayName;
            public ResourceTier Tier;
            public Color Color;
            public string Description;
            public string DroppedBy;
            public string CraftingUsage;

            public ItemSpec(string path, string id, string displayName, ResourceTier tier, Color color, string description, string droppedBy, string craftingUsage)
            {
                Path = path;
                Id = id;
                DisplayName = displayName;
                Tier = tier;
                Color = color;
                Description = description;
                DroppedBy = droppedBy;
                CraftingUsage = craftingUsage;
            }
        }

        private struct EnemySpec
        {
            public string Path;
            public string Id;
            public string DisplayName;
            public EnemyRank Rank;
            public ResourceTier Tier;
            public int Health;
            public int Damage;
            public float Speed;
            public float ChaseRange;
            public float AttackRange;
            public float Cooldown;
            public float Lunge;
            public float Knockback;
            public float CastRange;
            public int CastDamage;
            public float CastCooldown;
            public float CastChance;
            public Vector2 Scale;
            public Vector2 Collider;
            public Color Color;
            public string DropTablePath;
            public string DropTableId;
            public ItemDefinition DropItem;
            public int MinDrop;
            public int MaxDrop;

            public EnemySpec(
                string path,
                string id,
                string displayName,
                EnemyRank rank,
                ResourceTier tier,
                int health,
                int damage,
                float speed,
                float chaseRange,
                float attackRange,
                float cooldown,
                float lunge,
                float knockback,
                float castRange,
                int castDamage,
                float castCooldown,
                float castChance,
                Vector2 scale,
                Vector2 collider,
                Color color,
                string dropTablePath,
                ItemDefinition dropItem,
                int minDrop,
                int maxDrop)
            {
                Path = path;
                Id = id;
                DisplayName = displayName;
                Rank = rank;
                Tier = tier;
                Health = health;
                Damage = damage;
                Speed = speed;
                ChaseRange = chaseRange;
                AttackRange = attackRange;
                Cooldown = cooldown;
                Lunge = lunge;
                Knockback = knockback;
                CastRange = castRange;
                CastDamage = castDamage;
                CastCooldown = castCooldown;
                CastChance = castChance;
                Scale = scale;
                Collider = collider;
                Color = color;
                DropTablePath = dropTablePath;
                DropTableId = id + "_drops";
                DropItem = dropItem;
                MinDrop = minDrop;
                MaxDrop = maxDrop;
            }
        }

        static RouteProgressionDataAuthoring()
        {
            EditorApplication.delayCall += EnsureRouteProgressionData;
        }

        [MenuItem("Project Eclipse/Repair Route Progression Data")]
        public static void EnsureRouteProgressionData()
        {
            if (running)
            {
                return;
            }

            running = true;
            try
            {
                EnsureFolder(ItemsFolder);
                EnsureFolder(EnemiesFolder);
                EnsureFolder(DropTablesFolder);
                EnsureFolder(CreatureArtFolder);
                EnsureFolder(ItemArtFolder);

                bool changed = false;
                ItemDefinition sticks = AssetDatabase.LoadAssetAtPath<ItemDefinition>(ItemsFolder + "/TreeMaterial.asset");
                ItemDefinition birchLog = AssetDatabase.LoadAssetAtPath<ItemDefinition>(ItemsFolder + "/BirchLog.asset");
                ItemDefinition stone = AssetDatabase.LoadAssetAtPath<ItemDefinition>(ItemsFolder + "/Stone.asset");
                ItemDefinition coal = AssetDatabase.LoadAssetAtPath<ItemDefinition>(ItemsFolder + "/Coal.asset");
                ItemDefinition ironOre = AssetDatabase.LoadAssetAtPath<ItemDefinition>(ItemsFolder + "/IronOre.asset");
                ItemDefinition copperCore = AssetDatabase.LoadAssetAtPath<ItemDefinition>(ItemsFolder + "/CopperCore.asset");

                ItemDefinition pineLog = EnsureItem(new ItemSpec(
                    ItemsFolder + "/PineLog.asset",
                    "pine_log",
                    "Pine Log",
                    ResourceTier.Wood,
                    new Color(0.44f, 0.36f, 0.2f, 1f),
                    "Resin-scented wood gathered from the Pine route.",
                    "Pinelets, Pinelings, and Pinetrees",
                    "Future wood route crafting and stronger early handles."), ref changed);

                ItemDefinition tinOre = EnsureItem(new ItemSpec(
                    ItemsFolder + "/TinOre.asset",
                    "tin_ore",
                    "Tin Ore",
                    ResourceTier.Tin,
                    new Color(0.7f, 0.76f, 0.76f, 1f),
                    "Pale tin ore gathered after copper route progression opens.",
                    "Tin Orelets, Tin Orelings, and Tin Ore Nodes",
                    "Future bronze crafting, trinkets, and route upgrades."), ref changed);

                ItemDefinition zyncOre = EnsureItem(new ItemSpec(
                    ItemsFolder + "/ZyncOre.asset",
                    "zync_ore",
                    "Zync Ore",
                    ResourceTier.Zinc,
                    new Color(0.62f, 0.72f, 0.58f, 1f),
                    "Green-gray zync ore gathered after the tin route.",
                    "Zync Orelets, Zync Orelings, and Zync Ore Nodes",
                    "Future brass crafting, trinkets, and route upgrades."), ref changed);

                changed |= EnsureEnemyFamily("Birch", "birch", ResourceTier.Wood, birchLog, new Color(0.72f, 0.68f, 0.5f, 1f), new Color(0.46f, 0.66f, 0.36f, 1f), 8, 16, 30, 1.0f, 0.88f, 0.55f);
                changed |= EnsureEnemyFamily("Pine", "pine", ResourceTier.Wood, pineLog, new Color(0.34f, 0.24f, 0.14f, 1f), new Color(0.22f, 0.52f, 0.25f, 1f), 10, 20, 36, 1.0f, 0.9f, 0.58f);
                changed |= EnsureOreFamily("Rock", "rock", "rocks", ResourceTier.Stone, stone, new Color(0.48f, 0.5f, 0.52f, 1f), new Color(0.68f, 0.7f, 0.72f, 1f), 9, 18, 32);
                changed |= EnsureOreFamily("Coal", "coal", "coal", ResourceTier.Coal, coal, new Color(0.13f, 0.14f, 0.15f, 1f), new Color(0.78f, 0.55f, 0.32f, 1f), 12, 24, 40);
                changed |= EnsureEnemySpriteSheetOnly(EnemiesFolder + "/CopperCreature.asset", "copper_orelet");
                changed |= EnsureEnemySpriteSheetOnly(EnemiesFolder + "/CopperOreling.asset", "copper_oreling");
                changed |= EnsureEnemySpriteSheetOnly(EnemiesFolder + "/CopperOreNode.asset", "copper_ore_node");
                changed |= EnsureOreFamily("Tin", "tin", "tin", ResourceTier.Tin, tinOre, new Color(0.55f, 0.58f, 0.58f, 1f), new Color(0.82f, 0.9f, 0.9f, 1f), 16, 32, 52);
                changed |= EnsureOreFamily("Zync", "zync", "zync", ResourceTier.Zinc, zyncOre, new Color(0.44f, 0.5f, 0.45f, 1f), new Color(0.72f, 0.92f, 0.68f, 1f), 20, 40, 64);
                changed |= EnsureOreFamily("Iron", "iron", "iron", ResourceTier.Iron, ironOre, new Color(0.45f, 0.43f, 0.4f, 1f), new Color(0.73f, 0.48f, 0.32f, 1f), 26, 52, 84);

                if (sticks != null)
                {
                    changed |= EnsureDropTable(DropTablesFolder + "/TreeCreatureDropTable.asset", "sapling_drops", sticks, 1, 3);
                }

                if (copperCore != null)
                {
                    DropTableDefinition table = EnsureDropTable(DropTablesFolder + "/RouteGateSentinelDropTable.asset", "route_gate_sentinel_drops", copperCore, 1, 1, ref changed);
                    changed |= EnsureEnemy(new EnemySpec(
                        EnemiesFolder + "/RouteGateSentinel.asset",
                        "route_gate_sentinel",
                        "Route Gate Sentinel",
                        EnemyRank.MiniBoss,
                        ResourceTier.Copper,
                        140,
                        8,
                        0.72f,
                        7.2f,
                        1.25f,
                        1.65f,
                        0.65f,
                        5.8f,
                        6.5f,
                        5,
                        6.8f,
                        0.44f,
                        new Vector2(1.36f, 1.28f),
                        new Vector2(1.35f, 1.2f),
                        new Color(0.56f, 0.42f, 0.8f, 1f),
                        DropTablesFolder + "/RouteGateSentinelDropTable.asset",
                        copperCore,
                        1,
                        1), table);
                }

                if (changed)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("Project Eclipse repaired route progression item, drop table, and enemy data.");
                }
            }
            finally
            {
                running = false;
            }
        }

        private static bool EnsureEnemyFamily(string displayRoot, string idRoot, ResourceTier tier, ItemDefinition dropItem, Color bark, Color leaf, int baseHealth, int midHealth, int hardHealth, float baseScale, float midScale, float speed)
        {
            if (dropItem == null)
            {
                return false;
            }

            bool changed = false;
            DropTableDefinition letDrops = EnsureDropTable(DropTablesFolder + "/" + displayRoot + "letDropTable.asset", idRoot + "let_drops", dropItem, 1, 2, ref changed);
            DropTableDefinition lingDrops = EnsureDropTable(DropTablesFolder + "/" + displayRoot + "lingDropTable.asset", idRoot + "ling_drops", dropItem, 1, 3, ref changed);
            DropTableDefinition treeDrops = EnsureDropTable(DropTablesFolder + "/" + displayRoot + "treeDropTable.asset", idRoot + "tree_drops", dropItem, 2, 4, ref changed);

            changed |= EnsureEnemy(new EnemySpec(EnemiesFolder + "/" + displayRoot + "let.asset", idRoot + "let", displayRoot + "let", EnemyRank.Normal, tier, baseHealth, 1, speed + 0.18f, 5.2f, 0.82f, 1.25f, 0.5f, 2.4f, 0f, 1, 7f, 0.2f, new Vector2(baseScale * 0.82f, baseScale * 0.82f), new Vector2(0.66f, 0.58f), Color.Lerp(bark, leaf, 0.45f), DropTablesFolder + "/" + displayRoot + "letDropTable.asset", dropItem, 1, 2), letDrops);
            changed |= EnsureEnemy(new EnemySpec(EnemiesFolder + "/" + displayRoot + "ling.asset", idRoot + "ling", displayRoot + "ling", EnemyRank.Normal, tier, midHealth, 2, speed, 5.6f, 0.92f, 1.18f, 0.7f, 3f, 5.5f, 2, 6.4f, 0.34f, new Vector2(midScale, midScale), new Vector2(0.82f, 0.82f), leaf, DropTablesFolder + "/" + displayRoot + "lingDropTable.asset", dropItem, 1, 3), lingDrops);
            changed |= EnsureEnemy(new EnemySpec(EnemiesFolder + "/" + displayRoot + "tree.asset", idRoot + "tree", displayRoot + "tree", EnemyRank.Enhanced, tier, hardHealth, 3, Mathf.Max(0.35f, speed - 0.2f), 5.8f, 1.05f, 1.45f, 0.55f, 4.2f, 6f, 3, 7.2f, 0.38f, new Vector2(midScale * 1.28f, midScale * 1.18f), new Vector2(1.1f, 1.05f), Color.Lerp(bark, leaf, 0.25f), DropTablesFolder + "/" + displayRoot + "treeDropTable.asset", dropItem, 2, 4), treeDrops);
            return changed;
        }

        private static bool EnsureOreFamily(string displayRoot, string idRoot, string fileRoot, ResourceTier tier, ItemDefinition dropItem, Color rock, Color ore, int baseHealth, int midHealth, int hardHealth)
        {
            if (dropItem == null)
            {
                return false;
            }

            bool changed = false;
            string prefix = displayRoot.Replace(" ", string.Empty);
            string assetPrefix = displayRoot == "Rock" ? prefix : prefix + "Ore";
            string idPrefix = fileRoot == "rocks" ? "rock" : idRoot + "_ore";
            string displayPrefix = displayRoot == "Rock" ? "Rock" : displayRoot + " Ore";
            string letTableId = fileRoot == "rocks" ? "rocklet_drops" : idRoot + "_orelet_drops";
            string lingTableId = fileRoot == "rocks" ? "rockling_drops" : idRoot + "_oreling_drops";
            string nodeTableId = fileRoot == "rocks" ? "rock_node_drops" : idRoot + "_ore_node_drops";
            DropTableDefinition letDrops = EnsureDropTable(DropTablesFolder + "/" + assetPrefix + "letDropTable.asset", letTableId, dropItem, 1, 2, ref changed);
            DropTableDefinition lingDrops = EnsureDropTable(DropTablesFolder + "/" + assetPrefix + "lingDropTable.asset", lingTableId, dropItem, 1, 3, ref changed);
            DropTableDefinition nodeDrops = EnsureDropTable(DropTablesFolder + "/" + assetPrefix + "NodeDropTable.asset", nodeTableId, dropItem, 2, 4, ref changed);

            changed |= EnsureEnemy(new EnemySpec(EnemiesFolder + "/" + assetPrefix + "let.asset", idPrefix + "let", displayPrefix + "let", EnemyRank.Normal, tier, baseHealth, 1, 1.05f, 5.4f, 0.78f, 1.22f, 0.55f, 2.5f, 0f, 1, 7f, 0.2f, new Vector2(0.78f, 0.78f), new Vector2(0.7f, 0.55f), rock, DropTablesFolder + "/" + assetPrefix + "letDropTable.asset", dropItem, 1, 2), letDrops);
            changed |= EnsureEnemy(new EnemySpec(EnemiesFolder + "/" + assetPrefix + "ling.asset", idPrefix + "ling", displayPrefix + "ling", EnemyRank.Normal, tier, midHealth, 2, 0.92f, 5.8f, 0.92f, 1.18f, 0.72f, 3.2f, 5.2f, 2, 6.8f, 0.28f, new Vector2(1f, 1f), new Vector2(0.86f, 0.76f), Color.Lerp(rock, ore, 0.28f), DropTablesFolder + "/" + assetPrefix + "lingDropTable.asset", dropItem, 1, 3), lingDrops);
            changed |= EnsureEnemy(new EnemySpec(EnemiesFolder + "/" + assetPrefix + "Node.asset", idPrefix + "_node", displayPrefix + " Node", EnemyRank.Enhanced, tier, hardHealth, 3, 0.46f, 4.8f, 1.08f, 1.55f, 0.35f, 4.7f, 5.8f, 3, 8f, 0.36f, new Vector2(1.24f, 1.16f), new Vector2(1.28f, 0.84f), ore, DropTablesFolder + "/" + assetPrefix + "NodeDropTable.asset", dropItem, 2, 4), nodeDrops);
            return changed;
        }

        private static bool EnsureEnemySpriteSheetOnly(string enemyPath, string sheetId)
        {
            EnemyDefinition enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(enemyPath);
            if (enemy == null)
            {
                return false;
            }

            SerializedObject serialized = new SerializedObject(enemy);
            bool changed = SetObject(serialized, "spriteSheet", LoadCreatureSheet(sheetId));
            if (changed)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(enemy);
            }

            return changed;
        }

        private static ItemDefinition EnsureItem(ItemSpec spec, ref bool changed)
        {
            ItemDefinition item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(spec.Path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemDefinition>();
                item.name = GetAssetName(spec.Path);
                AssetDatabase.CreateAsset(item, spec.Path);
                changed = true;
            }
            else if (item.name != GetAssetName(spec.Path))
            {
                item.name = GetAssetName(spec.Path);
                EditorUtility.SetDirty(item);
                changed = true;
            }

            SerializedObject serialized = new SerializedObject(item);
            bool itemChanged = false;
            itemChanged |= SetString(serialized, "itemId", spec.Id);
            itemChanged |= SetString(serialized, "displayName", spec.DisplayName);
            itemChanged |= SetEnum(serialized, "category", (int)ItemCategory.Material);
            itemChanged |= SetEnum(serialized, "resourceTier", (int)spec.Tier);
            itemChanged |= SetInt(serialized, "stackLimit", 999);
            itemChanged |= SetColor(serialized, "placeholderColor", spec.Color);
            itemChanged |= SetString(serialized, "description", spec.Description);
            itemChanged |= SetString(serialized, "droppedBy", spec.DroppedBy);
            itemChanged |= SetString(serialized, "craftingUsage", spec.CraftingUsage);

            Sprite icon = LoadSprite(ItemArtFolder + "/" + spec.Id + "_icon.png", 64f);
            if (icon != null)
            {
                itemChanged |= SetObject(serialized, "icon", icon);
                itemChanged |= SetObject(serialized, "worldDropSprite", icon);
            }

            if (itemChanged)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(item);
                changed = true;
            }

            return item;
        }

        private static bool EnsureDropTable(string path, string id, ItemDefinition item, int min, int max)
        {
            bool changed = false;
            EnsureDropTable(path, id, item, min, max, ref changed);
            return changed;
        }

        private static DropTableDefinition EnsureDropTable(string path, string id, ItemDefinition item, int min, int max, ref bool changed)
        {
            DropTableDefinition table = AssetDatabase.LoadAssetAtPath<DropTableDefinition>(path);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance<DropTableDefinition>();
                table.name = GetAssetName(path);
                AssetDatabase.CreateAsset(table, path);
                changed = true;
            }
            else if (table.name != GetAssetName(path))
            {
                table.name = GetAssetName(path);
                EditorUtility.SetDirty(table);
                changed = true;
            }

            SerializedObject serialized = new SerializedObject(table);
            bool tableChanged = false;
            tableChanged |= SetString(serialized, "tableId", id);
            SerializedProperty entries = serialized.FindProperty("entries");
            if (entries != null)
            {
                if (entries.arraySize != 1)
                {
                    entries.arraySize = 1;
                    tableChanged = true;
                }

                SerializedProperty entry = entries.GetArrayElementAtIndex(0);
                tableChanged |= SetObject(entry, "item", item);
                tableChanged |= SetInt(entry, "minQuantity", min);
                tableChanged |= SetInt(entry, "maxQuantity", max);
                tableChanged |= SetFloat(entry, "chance", 1f);
            }

            SerializedProperty rareEntries = serialized.FindProperty("rareEntries");
            if (rareEntries != null && rareEntries.arraySize != 0)
            {
                rareEntries.arraySize = 0;
                tableChanged = true;
            }

            if (tableChanged)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(table);
                changed = true;
            }

            return table;
        }

        private static bool EnsureEnemy(EnemySpec spec, DropTableDefinition table)
        {
            EnemyDefinition enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(spec.Path);
            bool changed = false;
            if (enemy == null)
            {
                enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
                enemy.name = GetAssetName(spec.Path);
                AssetDatabase.CreateAsset(enemy, spec.Path);
                changed = true;
            }
            else if (enemy.name != GetAssetName(spec.Path))
            {
                enemy.name = GetAssetName(spec.Path);
                EditorUtility.SetDirty(enemy);
                changed = true;
            }

            SerializedObject serialized = new SerializedObject(enemy);
            changed |= SetString(serialized, "enemyId", spec.Id);
            changed |= SetString(serialized, "displayName", spec.DisplayName);
            changed |= SetEnum(serialized, "rank", (int)spec.Rank);
            changed |= SetEnum(serialized, "resourceTier", (int)spec.Tier);
            changed |= SetInt(serialized, "maxHealth", spec.Health);
            changed |= SetInt(serialized, "contactDamage", spec.Damage);
            changed |= SetFloat(serialized, "moveSpeed", spec.Speed);
            changed |= SetFloat(serialized, "chaseRange", spec.ChaseRange);
            changed |= SetFloat(serialized, "attackRange", spec.AttackRange);
            changed |= SetFloat(serialized, "attackCooldown", spec.Cooldown);
            changed |= SetFloat(serialized, "attackLungeForce", spec.Lunge);
            changed |= SetFloat(serialized, "attackKnockback", spec.Knockback);
            changed |= SetFloat(serialized, "rangedCastRange", spec.CastRange);
            changed |= SetInt(serialized, "rangedCastDamage", spec.CastDamage);
            changed |= SetFloat(serialized, "rangedCastCooldown", spec.CastCooldown);
            changed |= SetFloat(serialized, "rangedCastChance", spec.CastChance);
            changed |= SetFloat(serialized, "rangedProjectileSpeed", 5.4f);
            changed |= SetFloat(serialized, "rangedProjectileLifetime", 3f);
            changed |= SetInt(serialized, "ignorePlayerAboveGearScore", -1);
            changed |= SetVector2(serialized, "visualScale", spec.Scale);
            changed |= SetVector2(serialized, "colliderSize", spec.Collider);
            changed |= SetObject(serialized, "spriteSheet", LoadCreatureSheet(spec.Id));
            changed |= SetColor(serialized, "placeholderColor", spec.Color);
            changed |= SetObject(serialized, "dropTable", table);

            SerializedProperty drops = serialized.FindProperty("drops");
            if (drops != null)
            {
                if (drops.arraySize != 1)
                {
                    drops.arraySize = 1;
                    changed = true;
                }

                SerializedProperty drop = drops.GetArrayElementAtIndex(0);
                changed |= SetObject(drop, "item", spec.DropItem);
                changed |= SetInt(drop, "minQuantity", spec.MinDrop);
                changed |= SetInt(drop, "maxQuantity", spec.MaxDrop);
                changed |= SetFloat(drop, "chance", 1f);
            }

            if (changed)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(enemy);
            }

            return changed;
        }

        private static Texture2D LoadCreatureSheet(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId))
            {
                return null;
            }

            string path = CreatureArtFolder + "/" + enemyId + "_sheet.png";
            EnsureTextureImportSettings(path, 96f, true);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Sprite LoadSprite(string path, float pixelsPerUnit)
        {
            EnsureTextureImportSettings(path, pixelsPerUnit, false);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureTextureImportSettings(string path, float pixelsPerUnit, bool readable)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (!Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit))
            {
                importer.spritePixelsPerUnit = pixelsPerUnit;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (importer.isReadable != readable)
            {
                importer.isReadable = readable;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            int slash = path.LastIndexOf('/');
            string parent = slash > 0 ? path.Substring(0, slash) : "Assets";
            string child = slash > 0 ? path.Substring(slash + 1) : path;
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, child);
        }

        private static bool SetString(SerializedObject serialized, string name, string value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null || property.stringValue == value)
            {
                return false;
            }

            property.stringValue = value;
            return true;
        }

        private static bool SetInt(SerializedObject serialized, string name, int value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null || property.intValue == value)
            {
                return false;
            }

            property.intValue = value;
            return true;
        }

        private static bool SetFloat(SerializedObject serialized, string name, float value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }

        private static bool SetEnum(SerializedObject serialized, string name, int value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null || property.enumValueIndex == value)
            {
                return false;
            }

            property.enumValueIndex = value;
            return true;
        }

        private static bool SetColor(SerializedObject serialized, string name, Color value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null || property.colorValue == value)
            {
                return false;
            }

            property.colorValue = value;
            return true;
        }

        private static bool SetVector2(SerializedObject serialized, string name, Vector2 value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null || property.vector2Value == value)
            {
                return false;
            }

            property.vector2Value = value;
            return true;
        }

        private static bool SetObject(SerializedObject serialized, string name, Object value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool SetObject(SerializedProperty parent, string name, Object value)
        {
            SerializedProperty property = parent.FindPropertyRelative(name);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool SetInt(SerializedProperty parent, string name, int value)
        {
            SerializedProperty property = parent.FindPropertyRelative(name);
            if (property == null || property.intValue == value)
            {
                return false;
            }

            property.intValue = value;
            return true;
        }

        private static bool SetFloat(SerializedProperty parent, string name, float value)
        {
            SerializedProperty property = parent.FindPropertyRelative(name);
            if (property == null || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }

        private static string GetAssetName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            int slash = path.LastIndexOf('/');
            int dot = path.LastIndexOf('.');
            int start = slash >= 0 ? slash + 1 : 0;
            int length = dot > start ? dot - start : path.Length - start;
            return path.Substring(start, length);
        }
    }
}
#endif
