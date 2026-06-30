#if UNITY_EDITOR
using System.Collections.Generic;
using ProjectEclipse.Enemies;
using ProjectEclipse.Player;
using ProjectEclipse.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectEclipse.EditorTools
{
    [InitializeOnLoad]
    public static class MvpSceneRoadmapAuthoring
    {
        private const string MvpScenePath = "Assets/ProjectEclipse/Scenes/ProjectEclipse_MVP.unity";
        private const string MapRootName = "Scene Authored Route Map";
        private const string RuntimeMapRootName = "Editable MVP Map";
        private const float RoomWidth = 24f;
        private const float RoomHeight = 11.5f;
        private const float HorizontalRoomSpacing = 36f;
        private const float VerticalRoomSpacing = 18f;
        private const float FloorSurfaceOffsetY = -2.15f;
        private const float FloorThickness = 0.6f;
        private const float GroundFillVisualHeight = 0.56f;
        private const float OneWayPlatformVisualHeight = 0.24f;
        private const float StandingSurfaceClearance = 0.01f;
        private const float UpperPlatformSurfaceOffsetY = FloorSurfaceOffsetY + 3.15f;
        private const float SidePortalInset = 2.8f;
        private const float PlayerColliderWidth = 0.5f;
        private const float PlayerColliderHeight = 1f;
        private const float PlayerColliderOffsetY = 0.45f;
        private const float PortalHeight = 1.55f;
        private const float PortalWidth = 0.9f;
        private const int RouteDepthCount = 5;
        private const int ExpectedRouteRoomCount = 51;
        private const int ExpectedPortalCount = 100;

        private static readonly string[] HorizontalBaseOrder =
        {
            "pine",
            "birch",
            "saplings",
            "safe",
            "rocks",
            "coal",
            "copper",
            "tin",
            "zync",
            "miniboss",
            "iron"
        };

        private enum PortalSide
        {
            West,
            East,
            Up,
            Down
        }

        private struct RoomSpec
        {
            public string Id;
            public string Name;
            public string Route;
            public int Depth;
            public Vector2 Center;
            public Color Sky;
            public Color Ground;
            public Color Platform;
            public Color Portal;
            public string BackdropPath;
            public string GroundPath;
            public string PlatformPath;

            public RoomSpec(string id, string name, string route, int depth, Vector2 center, Color sky, Color ground, Color platform, Color portal, string backdropPath, string groundPath, string platformPath)
            {
                Id = id;
                Name = name;
                Route = route;
                Depth = depth;
                Center = center;
                Sky = sky;
                Ground = ground;
                Platform = platform;
                Portal = portal;
                BackdropPath = backdropPath;
                GroundPath = groundPath;
                PlatformPath = platformPath;
            }
        }

        private struct RouteSpec
        {
            public string Id;
            public string DisplayName;
            public int HorizontalIndex;
            public Color Sky;
            public Color Ground;
            public Color Platform;
            public Color Portal;
            public string BackdropPath;
            public string GroundPath;
            public string PlatformPath;
            public string BaseEnemyId;
            public string MidEnemyId;
            public string HardEnemyId;
            public string[] DepthNames;

            public RouteSpec(string id, string displayName, int horizontalIndex, Color sky, Color ground, Color platform, Color portal, string backdropPath, string groundPath, string platformPath, string baseEnemyId, string midEnemyId, string hardEnemyId, string[] depthNames)
            {
                Id = id;
                DisplayName = displayName;
                HorizontalIndex = horizontalIndex;
                Sky = sky;
                Ground = ground;
                Platform = platform;
                Portal = portal;
                BackdropPath = backdropPath;
                GroundPath = groundPath;
                PlatformPath = platformPath;
                BaseEnemyId = baseEnemyId;
                MidEnemyId = midEnemyId;
                HardEnemyId = hardEnemyId;
                DepthNames = depthNames;
            }
        }

        static MvpSceneRoadmapAuthoring()
        {
            EditorApplication.delayCall += AutoRebuildIfFlatLegacyMap;
        }

        [MenuItem("Project Eclipse/Rebuild MVP Route Map")]
        public static void RebuildMvpRouteMap()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Scene scene = EnsureMvpSceneOpen();
            if (!scene.isLoaded)
            {
                return;
            }

            BuildSceneRouteMap(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Project Eclipse rebuilt the MVP scene-authored route/depth map.");
        }

        private static void AutoRebuildIfFlatLegacyMap()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded)
            {
                return;
            }

            string activeScenePath = NormalizeScenePath(scene.path);
            if (activeScenePath != MvpScenePath)
            {
                if (!IsUnityPlayModeBackupScene(activeScenePath) || AssetDatabase.LoadAssetAtPath<SceneAsset>(MvpScenePath) == null)
                {
                    return;
                }

                scene = EditorSceneManager.OpenScene(MvpScenePath);
                if (!scene.isLoaded)
                {
                    return;
                }
            }

            GameObject existingMapRoot = GameObject.Find(MapRootName);
            if (existingMapRoot != null && !IsCurrentMapOutdated(existingMapRoot))
            {
                return;
            }

            if (existingMapRoot == null && !HasLegacyFlatMap(scene))
            {
                return;
            }

            BuildSceneRouteMap(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Project Eclipse converted the MVP scene into the route/depth map.");
        }

        private static bool IsUnityPlayModeBackupScene(string scenePath)
        {
            return !string.IsNullOrEmpty(scenePath)
                && scenePath.Replace('\\', '/').StartsWith("Temp/__Backupscenes/", System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCurrentMapOutdated(GameObject mapRoot)
        {
            if (mapRoot == null)
            {
                return true;
            }

            int roomCount = mapRoot.GetComponentsInChildren<MapArea2D>(true).Length;
            int portalCount = mapRoot.GetComponentsInChildren<RoomPortal2D>(true).Length;
            if (roomCount != ExpectedRouteRoomCount || portalCount != ExpectedPortalCount)
            {
                return true;
            }

            Transform saplingsUpPortal = mapRoot.transform.Find("saplings-d1-up-portal");
            float expectedUpPortalY = UpperPlatformSurfaceOffsetY + 0.75f;
            if (saplingsUpPortal == null || Mathf.Abs(saplingsUpPortal.position.y - expectedUpPortalY) > 0.05f)
            {
                return true;
            }

            BoxCollider2D safeFloor = FindColliderNamed(mapRoot, "safe-floor");
            if (safeFloor == null || safeFloor.size.y < FloorThickness - 0.01f)
            {
                return true;
            }

            SpriteRenderer platformArt = FindRendererNamed(mapRoot, "saplings-d1-upper-step-art");
            if (platformArt == null || platformArt.drawMode != SpriteDrawMode.Tiled || Mathf.Abs(platformArt.size.y - OneWayPlatformVisualHeight) > 0.01f)
            {
                return true;
            }

            SpriteRenderer groundFill = FindRendererNamed(mapRoot, "Ground Fill");
            if (groundFill == null || groundFill.drawMode != SpriteDrawMode.Tiled || Mathf.Abs(groundFill.size.y - GroundFillVisualHeight) > 0.01f)
            {
                return true;
            }

            Transform safeEastPortal = mapRoot.transform.Find("safe-east-portal");
            if (safeEastPortal == null || Mathf.Abs(safeEastPortal.position.x - (RoomWidth * 0.5f - SidePortalInset)) > 0.05f)
            {
                return true;
            }

            Transform safePlayerSpawn = mapRoot.transform.Find("safe-player-spawn");
            float expectedPlayerSpawnY = FloorSurfaceOffsetY + GetPlayerFeetOffset() + StandingSurfaceClearance;
            if (safePlayerSpawn == null || Mathf.Abs(safePlayerSpawn.position.y - expectedPlayerSpawnY) > 0.05f)
            {
                return true;
            }

            return !HasGeneratedChildNamed(mapRoot, "saplings-d1-upper-step");
        }

        private static BoxCollider2D FindColliderNamed(GameObject root, string objectName)
        {
            BoxCollider2D[] colliders = root.GetComponentsInChildren<BoxCollider2D>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && colliders[i].name == objectName)
                {
                    return colliders[i];
                }
            }

            return null;
        }

        private static SpriteRenderer FindRendererNamed(GameObject root, string objectName)
        {
            SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].name == objectName)
                {
                    return renderers[i];
                }
            }

            return null;
        }

        private static bool HasGeneratedChildNamed(GameObject root, string objectName)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null && children[i].name == objectName)
                {
                    return true;
                }
            }

            return false;
        }

        private static Scene EnsureMvpSceneOpen()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (NormalizeScenePath(activeScene.path) == MvpScenePath)
            {
                return activeScene;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MvpScenePath) == null)
            {
                Debug.LogWarning("Project Eclipse MVP scene is missing at " + MvpScenePath + ".");
                return default;
            }

            return EditorSceneManager.OpenScene(MvpScenePath);
        }

        private static bool HasLegacyFlatMap(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == null)
                {
                    continue;
                }

                if (IsLegacyMapRoot(root.name) || root.GetComponentInChildren<EnemyController>(true) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static void BuildSceneRouteMap(Scene scene)
        {
            ClearLegacyAndGeneratedMap(scene);

            GameObject mapRoot = new GameObject(MapRootName);
            Undo.RegisterCreatedObjectUndo(mapRoot, "Create MVP route map");
            SceneManager.MoveGameObjectToScene(mapRoot, scene);

            Dictionary<string, RoomBounds2D> rooms = new Dictionary<string, RoomBounds2D>();
            Dictionary<string, RoomSpec> specsById = new Dictionary<string, RoomSpec>();
            RoomSpec[] specs = CreateRoomSpecs();
            for (int i = 0; i < specs.Length; i++)
            {
                RoomSpec spec = specs[i];
                specsById[spec.Id] = spec;
                rooms[spec.Id] = CreateRoom(mapRoot.transform, spec);
            }

            for (int i = 0; i < HorizontalBaseOrder.Length - 1; i++)
            {
                LinkRooms(mapRoot.transform, specsById, rooms, GetDepthRoomId(HorizontalBaseOrder[i], 1), PortalSide.East, GetDepthRoomId(HorizontalBaseOrder[i + 1], 1), PortalSide.West);
            }

            RouteSpec[] routes = CreateRouteSpecs();
            for (int i = 0; i < routes.Length; i++)
            {
                for (int depth = 1; depth < RouteDepthCount; depth++)
                {
                    LinkRooms(mapRoot.transform, specsById, rooms, GetDepthRoomId(routes[i].Id, depth), PortalSide.Up, GetDepthRoomId(routes[i].Id, depth + 1), PortalSide.Down);
                }
            }

            ConfigureGameManagerAndPlayer(scene, mapRoot);
        }

        private static RoomSpec[] CreateRoomSpecs()
        {
            string forestBackdrop = "Assets/ProjectEclipse/Art/World/forest_area_backdrop.png";
            string forestGround = "Assets/ProjectEclipse/Art/World/forest_ground.png";
            string forestPlatform = "Assets/ProjectEclipse/Art/World/forest_platform.png";

            List<RoomSpec> rooms = new List<RoomSpec>
            {
                new RoomSpec("safe", "Safe Zone", "Safe Zone", 0, new Vector2(0f, 0f), new Color(0.36f, 0.58f, 0.58f), new Color(0.28f, 0.38f, 0.24f), new Color(0.34f, 0.28f, 0.18f), new Color(0.75f, 0.86f, 0.58f), forestBackdrop, forestGround, forestPlatform)
            };

            RouteSpec[] routes = CreateRouteSpecs();
            for (int i = 0; i < routes.Length; i++)
            {
                RouteSpec route = routes[i];
                for (int depth = 1; depth <= RouteDepthCount; depth++)
                {
                    int depthIndex = depth - 1;
                    float depthTint = depthIndex * 0.045f;
                    string depthName = route.DepthNames != null && depthIndex < route.DepthNames.Length ? route.DepthNames[depthIndex] : "Depth " + depth.ToString();
                    rooms.Add(new RoomSpec(
                        GetDepthRoomId(route.Id, depth),
                        route.DisplayName + " Route Depth " + depth.ToString() + " - " + depthName,
                        route.DisplayName + " Route",
                        depth,
                        new Vector2(route.HorizontalIndex * HorizontalRoomSpacing, depthIndex * VerticalRoomSpacing),
                        Color.Lerp(route.Sky, Color.black, depthTint),
                        Color.Lerp(route.Ground, Color.black, depthTint),
                        Color.Lerp(route.Platform, Color.white, depthIndex * 0.035f),
                        Color.Lerp(route.Portal, Color.white, depthIndex * 0.04f),
                        route.BackdropPath,
                        route.GroundPath,
                        route.PlatformPath));
                }
            }

            return rooms.ToArray();
        }

        private static RouteSpec[] CreateRouteSpecs()
        {
            string forestBackdrop = "Assets/ProjectEclipse/Art/World/forest_area_backdrop.png";
            string forestGround = "Assets/ProjectEclipse/Art/World/forest_ground.png";
            string forestPlatform = "Assets/ProjectEclipse/Art/World/forest_platform.png";
            string stoneBackdrop = "Assets/ProjectEclipse/Art/World/stone_area_backdrop.png";
            string stonePlatform = "Assets/ProjectEclipse/Art/World/stone_platform.png";
            string coalBackdrop = "Assets/ProjectEclipse/Art/World/coal_area_backdrop.png";
            string coalPlatform = "Assets/ProjectEclipse/Art/World/coal_platform.png";
            string copperBackdrop = "Assets/ProjectEclipse/Art/World/copper_area_backdrop.png";
            string copperPlatform = "Assets/ProjectEclipse/Art/World/copper_platform.png";

            return new[]
            {
                new RouteSpec("saplings", "Saplings", -1, new Color(0.25f, 0.5f, 0.47f), new Color(0.23f, 0.37f, 0.2f), new Color(0.39f, 0.28f, 0.16f), new Color(0.42f, 0.86f, 0.48f), forestBackdrop, forestGround, forestPlatform, "sapling", "birchling", "birchling", new[] { "Saplings", "Saplings + Birchlings", "Birchlings", "Birchlings + Pine", "Pine Stand" }),
                new RouteSpec("birch", "Birch", -2, new Color(0.28f, 0.52f, 0.45f), new Color(0.24f, 0.36f, 0.19f), new Color(0.53f, 0.42f, 0.24f), new Color(0.64f, 0.9f, 0.58f), forestBackdrop, forestGround, forestPlatform, "birchling", "birchling", "birchling", new[] { "Birchlings", "Thick Birchlings", "Birch Grove", "Dense Birch Grove", "Old Birch Grove" }),
                new RouteSpec("pine", "Pine", -3, new Color(0.22f, 0.46f, 0.43f), new Color(0.18f, 0.32f, 0.18f), new Color(0.3f, 0.22f, 0.14f), new Color(0.38f, 0.8f, 0.43f), forestBackdrop, forestGround, forestPlatform, "sapling", "birchling", "birchling", new[] { "Pine Saplings", "Pine + Birchlings", "Pine Timber", "Pine Timber + Heartwood", "Heartwood Stand" }),
                new RouteSpec("rocks", "Rocks", 1, new Color(0.31f, 0.39f, 0.43f), new Color(0.31f, 0.31f, 0.32f), new Color(0.43f, 0.43f, 0.43f), new Color(0.66f, 0.74f, 0.86f), stoneBackdrop, forestGround, stonePlatform, "rock_creature", "rock_creature", "copper_orelet", new[] { "Rocklets", "Rocklets + Rocklings", "Rocklings", "Rocklings + Dense Rock", "Dense Rock Cluster" }),
                new RouteSpec("coal", "Coal", 2, new Color(0.26f, 0.3f, 0.34f), new Color(0.21f, 0.21f, 0.2f), new Color(0.36f, 0.35f, 0.34f), new Color(0.84f, 0.62f, 0.42f), coalBackdrop, forestGround, coalPlatform, "coal_sprite", "coal_sprite", "coal_sprite", new[] { "Coal Sprites", "Coal Sprites + Dense Coal", "Dense Coal", "Dense Coal + Coal Nodes", "Coal Node Cluster" }),
                new RouteSpec("copper", "Copper", 3, new Color(0.3f, 0.32f, 0.34f), new Color(0.25f, 0.22f, 0.2f), new Color(0.56f, 0.34f, 0.22f), new Color(0.95f, 0.55f, 0.28f), copperBackdrop, forestGround, copperPlatform, "copper_orelet", "copper_oreling", "copper_ore_node", new[] { "Copper Orelets", "Orelets + Orelings", "Copper Orelings", "Orelings + Ore Nodes", "Copper Ore Nodes" }),
                new RouteSpec("tin", "Tin", 4, new Color(0.32f, 0.39f, 0.43f), new Color(0.28f, 0.28f, 0.3f), new Color(0.48f, 0.5f, 0.5f), new Color(0.78f, 0.86f, 0.9f), stoneBackdrop, forestGround, stonePlatform, "copper_orelet", "copper_oreling", "copper_ore_node", new[] { "Tin Chips", "Tin Chips + Tinlings", "Tinlings", "Tinlings + Tin Nodes", "Tin Node Cluster" }),
                new RouteSpec("zync", "Zync", 5, new Color(0.3f, 0.36f, 0.4f), new Color(0.25f, 0.26f, 0.27f), new Color(0.47f, 0.51f, 0.46f), new Color(0.72f, 0.9f, 0.72f), stoneBackdrop, forestGround, stonePlatform, "copper_orelet", "copper_oreling", "copper_ore_node", new[] { "Zync Chips", "Zync Chips + Zynclings", "Zynclings", "Zynclings + Zync Nodes", "Zync Node Cluster" }),
                new RouteSpec("miniboss", "Mini Boss", 6, new Color(0.28f, 0.26f, 0.33f), new Color(0.25f, 0.23f, 0.27f), new Color(0.45f, 0.39f, 0.52f), new Color(0.75f, 0.64f, 1f), copperBackdrop, forestGround, copperPlatform, "copper_orelet", "copper_oreling", "copper_ore_node", new[] { "Route Gate", "Gate Approach", "Gate Pressure", "Gate Guards", "Mini Boss Gate" }),
                new RouteSpec("iron", "Iron Ore", 7, new Color(0.27f, 0.31f, 0.34f), new Color(0.23f, 0.23f, 0.24f), new Color(0.42f, 0.4f, 0.39f), new Color(0.74f, 0.76f, 0.8f), stoneBackdrop, forestGround, stonePlatform, "copper_orelet", "copper_oreling", "copper_ore_node", new[] { "Iron Chips", "Iron Chips + Ironlings", "Ironlings", "Ironlings + Iron Nodes", "Iron Node Cluster" })
            };
        }

        private static string GetDepthRoomId(string routeId, int depth)
        {
            return routeId == "safe" ? "safe" : routeId + "-d" + depth.ToString();
        }

        private static bool TryGetRouteSpec(string routeId, out RouteSpec route)
        {
            RouteSpec[] routes = CreateRouteSpecs();
            for (int i = 0; i < routes.Length; i++)
            {
                if (routes[i].Id == routeId)
                {
                    route = routes[i];
                    return true;
                }
            }

            route = default;
            return false;
        }

        private static string GetRouteId(string roomId)
        {
            int depthIndex = roomId.LastIndexOf("-d");
            return depthIndex > 0 ? roomId.Substring(0, depthIndex) : roomId;
        }

        private static RoomBounds2D CreateRoom(Transform mapRoot, RoomSpec spec)
        {
            GameObject room = new GameObject(spec.Name);
            room.transform.SetParent(mapRoot);
            room.transform.position = new Vector3(spec.Center.x, spec.Center.y, 0f);

            RoomBounds2D bounds = room.AddComponent<RoomBounds2D>();
            bounds.Configure(new Vector2(RoomWidth, RoomHeight));
            MapArea2D area = room.AddComponent<MapArea2D>();
            area.Configure(spec.Id, spec.Name, new Vector2(RoomWidth, RoomHeight));

            CreateSprite(room.transform, "Backdrop", Vector3.forward * 2.5f, new Vector3(6.2f, 4.4f, 1f), spec.BackdropPath, spec.Sky, -40);
            CreateTiledSprite(room.transform, "Ground Fill", new Vector3(0f, FloorSurfaceOffsetY - GroundFillVisualHeight * 0.5f, 1.8f), new Vector2(RoomWidth + 0.8f, GroundFillVisualHeight), spec.GroundPath, spec.Ground, -18);
            CreateFloor(room.transform, spec);
            CreateRoomPlatforms(room.transform, spec);
            CreateSpawn(room.transform, spec);
            CreateEnemySpawns(room.transform, spec);
            return bounds;
        }

        private static void CreateFloor(Transform room, RoomSpec spec)
        {
            GameObject floor = new GameObject(spec.Id + "-floor");
            floor.transform.SetParent(room);
            floor.transform.localPosition = new Vector3(0f, FloorSurfaceOffsetY - FloorThickness * 0.5f, 0f);

            BoxCollider2D collider = floor.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(RoomWidth + 0.8f, FloorThickness);
            floor.AddComponent<PlatformSurface>();
            MapPlatform2D platform = floor.AddComponent<MapPlatform2D>();
            platform.Configure(spec.Id + "-floor", RoomWidth + 0.8f, false, FloorThickness);
        }

        private static void CreateRoomPlatforms(Transform room, RoomSpec spec)
        {
            if (spec.Id == "safe")
            {
                return;
            }

            CreateOneWayPlatform(room, spec.Id + "-lower-step", new Vector2(-3.6f, FloorSurfaceOffsetY + 1.05f), 4.2f, spec.PlatformPath, spec.Platform);
            CreateOneWayPlatform(room, spec.Id + "-mid-step", new Vector2(1.2f, FloorSurfaceOffsetY + 2.05f), 4.2f, spec.PlatformPath, Lighten(spec.Platform, 0.08f));
            CreateOneWayPlatform(room, spec.Id + "-upper-step", new Vector2(RoomWidth * 0.32f, UpperPlatformSurfaceOffsetY), 3.8f, spec.PlatformPath, Lighten(spec.Platform, 0.16f));
        }

        private static void CreateOneWayPlatform(Transform room, string id, Vector2 center, float width, string spritePath, Color color)
        {
            CreateTiledSprite(room, id + "-art", new Vector3(center.x, center.y + 0.04f - OneWayPlatformVisualHeight * 0.5f, 1.55f), new Vector2(width, OneWayPlatformVisualHeight), spritePath, color, -12);

            GameObject surface = new GameObject(id);
            surface.transform.SetParent(room);
            surface.transform.localPosition = new Vector3(center.x, center.y, 0f);
            BoxCollider2D collider = surface.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(width, 0.08f);
            surface.AddComponent<OneWayPlatform>();
            surface.AddComponent<PlatformSurface>();
            MapPlatform2D platform = surface.AddComponent<MapPlatform2D>();
            platform.Configure(id, width, true, 0.08f);
        }

        private static void CreateSpawn(Transform room, RoomSpec spec)
        {
            GameObject spawn = new GameObject(spec.Id + "-player-spawn");
            spawn.transform.SetParent(room);
            float x = spec.Id == "safe" ? -1.8f : -RoomWidth * 0.5f + 1.2f;
            spawn.transform.localPosition = new Vector3(x, FloorSurfaceOffsetY + GetPlayerFeetOffset() + StandingSurfaceClearance, 0f);
        }

        private static void CreateEnemySpawns(Transform room, RoomSpec spec)
        {
            if (spec.Id == "safe")
            {
                return;
            }

            RouteSpec route;
            if (!TryGetRouteSpec(GetRouteId(spec.Id), out route))
            {
                return;
            }

            switch (spec.Depth)
            {
                case 1:
                    CreateEnemySpawn(room, route.BaseEnemyId, new Vector2(-2.8f, FloorSurfaceOffsetY), 4, 3.4f, 4.2f, 1.1f);
                    break;
                case 2:
                    CreateEnemySpawn(room, route.BaseEnemyId, new Vector2(-4.2f, FloorSurfaceOffsetY), 3, 3.2f, 4.1f, 1f);
                    CreateEnemySpawn(room, route.MidEnemyId, new Vector2(3.1f, FloorSurfaceOffsetY), 2, 2.8f, 5.2f, 1.2f);
                    break;
                case 3:
                    CreateEnemySpawn(room, route.MidEnemyId, new Vector2(-1.2f, FloorSurfaceOffsetY), 5, 4.4f, 5.4f, 1.4f);
                    break;
                case 4:
                    CreateEnemySpawn(room, route.MidEnemyId, new Vector2(-4.1f, FloorSurfaceOffsetY), 3, 3.4f, 5.6f, 1.3f);
                    CreateEnemySpawn(room, route.HardEnemyId, new Vector2(3.7f, FloorSurfaceOffsetY), 2, 3f, 7.2f, 1.5f);
                    break;
                case 5:
                    CreateEnemySpawn(room, route.HardEnemyId, new Vector2(-1f, FloorSurfaceOffsetY), 5, 4.6f, 7.8f, 1.7f);
                    break;
            }
        }

        private static void CreateEnemySpawn(Transform room, string enemyId, Vector2 footPosition, int maxAlive, float radius, float respawn, float jitter)
        {
            if (string.IsNullOrEmpty(enemyId))
            {
                return;
            }

            EnemyDefinition definition = FindEnemyDefinition(enemyId);
            if (definition == null)
            {
                return;
            }

            GameObject spawn = new GameObject(enemyId + "-spawn");
            spawn.transform.SetParent(room);
            float centerYOffset = Mathf.Max(0.15f, definition.ColliderSize.y * Mathf.Abs(definition.VisualScale.y) * 0.5f);
            spawn.transform.localPosition = new Vector3(footPosition.x, footPosition.y + centerYOffset + StandingSurfaceClearance, 0f);
            EnemySpawnPoint2D spawnPoint = spawn.AddComponent<EnemySpawnPoint2D>();
            spawnPoint.Configure(definition, maxAlive, radius, respawn, jitter);
        }

        private static void LinkRooms(Transform mapRoot, Dictionary<string, RoomSpec> specs, Dictionary<string, RoomBounds2D> rooms, string fromId, PortalSide fromSide, string toId, PortalSide toSide)
        {
            RoomSpec fromSpec;
            RoomSpec toSpec;
            RoomBounds2D fromRoom;
            RoomBounds2D toRoom;
            if (!specs.TryGetValue(fromId, out fromSpec) || !specs.TryGetValue(toId, out toSpec) || !rooms.TryGetValue(fromId, out fromRoom) || !rooms.TryGetValue(toId, out toRoom))
            {
                return;
            }

            RoomPortal2D first = CreatePortal(mapRoot, fromSpec, fromSide, fromRoom);
            RoomPortal2D second = CreatePortal(mapRoot, toSpec, toSide, toRoom);
            first.LinkTo(second);
            second.LinkTo(first);
        }

        private static RoomPortal2D CreatePortal(Transform mapRoot, RoomSpec spec, PortalSide side, RoomBounds2D room)
        {
            Vector2 position = GetPortalPosition(spec, side);
            GameObject portal = new GameObject(spec.Id + "-" + side.ToString().ToLowerInvariant() + "-portal");
            portal.transform.SetParent(mapRoot);
            portal.transform.position = new Vector3(position.x, position.y, 0f);
            portal.transform.localScale = new Vector3(PortalWidth, PortalHeight, 1f);

            CreateSprite(portal.transform, "Teleport Pad", new Vector3(0f, -0.5f, 0.02f), new Vector3(1.15f, 0.5f, 1f), spec.PlatformPath, spec.Portal, 1);
            CreateSprite(portal.transform, "Teleport Column", new Vector3(0f, -0.08f, 0.01f), new Vector3(0.45f, 1.1f, 1f), spec.PlatformPath, Lighten(spec.Portal, 0.18f), 2);

            BoxCollider2D trigger = portal.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = Vector2.one;

            GameObject arrival = new GameObject("Arrival Point");
            arrival.transform.SetParent(portal.transform);
            Vector3 arrivalWorld = GetArrivalPosition(spec, side);
            arrival.transform.position = arrivalWorld;

            RoomPortal2D link = portal.AddComponent<RoomPortal2D>();
            link.Configure(room, arrival.transform, null);
            return link;
        }

        private static Vector2 GetPortalPosition(RoomSpec spec, PortalSide side)
        {
            float x;
            switch (side)
            {
                case PortalSide.West:
                    x = spec.Center.x - RoomWidth * 0.5f + SidePortalInset;
                    break;
                case PortalSide.East:
                    x = spec.Center.x + RoomWidth * 0.5f - SidePortalInset;
                    break;
                case PortalSide.Up:
                    x = spec.Center.x + RoomWidth * 0.32f;
                    break;
                case PortalSide.Down:
                    x = spec.Center.x - RoomWidth * 0.32f;
                    break;
                default:
                    x = spec.Center.x;
                    break;
            }

            float surfaceY = side == PortalSide.Up ? UpperPlatformSurfaceOffsetY : FloorSurfaceOffsetY;
            return new Vector2(x, spec.Center.y + surfaceY + 0.75f);
        }

        private static Vector3 GetArrivalPosition(RoomSpec spec, PortalSide side)
        {
            Vector2 portal = GetPortalPosition(spec, side);
            float xOffset;
            switch (side)
            {
                case PortalSide.West:
                    xOffset = 1.15f;
                    break;
                case PortalSide.East:
                    xOffset = -1.15f;
                    break;
                case PortalSide.Up:
                    xOffset = -1.05f;
                    break;
                case PortalSide.Down:
                    xOffset = 1.05f;
                    break;
                default:
                    xOffset = 1.05f;
                    break;
            }

            float surfaceY = side == PortalSide.Up ? UpperPlatformSurfaceOffsetY : FloorSurfaceOffsetY;
            return new Vector3(portal.x + xOffset, spec.Center.y + surfaceY + GetPlayerFeetOffset() + StandingSurfaceClearance, 0f);
        }

        private static GameObject CreateSprite(Transform parent, string name, Vector3 localPosition, Vector3 scale, string spritePath, Color color, int sortingOrder)
        {
            GameObject spriteObject = new GameObject(name);
            spriteObject.transform.SetParent(parent);
            spriteObject.transform.localPosition = localPosition;
            spriteObject.transform.localScale = scale;

            SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return spriteObject;
        }

        private static GameObject CreateTiledSprite(Transform parent, string name, Vector3 localPosition, Vector2 size, string spritePath, Color color, int sortingOrder)
        {
            GameObject spriteObject = new GameObject(name);
            spriteObject.transform.SetParent(parent);
            spriteObject.transform.localPosition = localPosition;
            spriteObject.transform.localScale = Vector3.one;

            SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = size;
            return spriteObject;
        }

        private static void ClearLegacyAndGeneratedMap(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            List<GameObject> destroy = new List<GameObject>();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == null)
                {
                    continue;
                }

                if (root.name == MapRootName || root.name == RuntimeMapRootName || IsLegacyMapRoot(root.name) || root.GetComponentInChildren<EnemyController>(true) != null)
                {
                    destroy.Add(root);
                }
            }

            for (int i = 0; i < destroy.Count; i++)
            {
                Undo.DestroyObjectImmediate(destroy[i]);
            }
        }

        private static bool IsLegacyMapRoot(string name)
        {
            switch (name)
            {
                case "Sapling Grove":
                case "Rock Creature Area":
                case "Birchling Canopy":
                case "Copper and Coal Route":
                case "Ground":
                case "Sapling Branch Platform":
                case "Rock Creature Platform":
                case "Coal Ridge":
                case "Copper Shelf":
                case "Birch Perch Platform":
                    return true;
                default:
                    return false;
            }
        }

        private static void ConfigureGameManagerAndPlayer(Scene scene, GameObject mapRoot)
        {
            MvpGameManager gameManager = Object.FindAnyObjectByType<MvpGameManager>();
            if (gameManager != null)
            {
                MvpRoomFlowBuilder builder = gameManager.GetComponent<MvpRoomFlowBuilder>();
                if (builder == null)
                {
                    builder = gameManager.gameObject.AddComponent<MvpRoomFlowBuilder>();
                }

                SerializedObject builderObject = new SerializedObject(builder);
                SerializedProperty buildOnStart = builderObject.FindProperty("buildOnStart");
                if (buildOnStart != null)
                {
                    buildOnStart.boolValue = false;
                }
                builderObject.ApplyModifiedPropertiesWithoutUndo();

                SerializedObject managerObject = new SerializedObject(gameManager);
                SerializedProperty roomFlowBuilder = managerObject.FindProperty("roomFlowBuilder");
                if (roomFlowBuilder != null)
                {
                    roomFlowBuilder.objectReferenceValue = builder;
                }

                SerializedProperty placedEnemies = managerObject.FindProperty("placedEnemies");
                if (placedEnemies != null)
                {
                    placedEnemies.arraySize = 0;
                }
                managerObject.ApplyModifiedPropertiesWithoutUndo();
            }

            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                NormalizePlayerCollider(player);
                player.transform.position = new Vector3(-1.8f, FloorSurfaceOffsetY + GetPlayerFeetOffset() + StandingSurfaceClearance, 0f);
                Rigidbody2D body = player.GetComponent<Rigidbody2D>();
                if (body != null)
                {
                    body.linearVelocity = Vector2.zero;
                }
            }
        }

        private static EnemyDefinition FindEnemyDefinition(string enemyId)
        {
            string[] guids = AssetDatabase.FindAssets("t:EnemyDefinition", new[] { "Assets/ProjectEclipse/Data/Enemies" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EnemyDefinition definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
                if (definition != null && definition.EnemyId.ToLowerInvariant() == enemyId.ToLowerInvariant())
                {
                    return definition;
                }
            }

            return null;
        }

        private static float GetPlayerFeetOffset()
        {
            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                NormalizePlayerCollider(player);
            }

            Collider2D collider = player != null ? player.GetComponent<Collider2D>() : null;
            if (player == null || collider == null)
            {
                return 0.72f;
            }

            return Mathf.Max(0.05f, player.transform.position.y - collider.bounds.min.y);
        }

        private static void NormalizePlayerCollider(PlayerController player)
        {
            BoxCollider2D box = player != null ? player.GetComponent<BoxCollider2D>() : null;
            if (box == null)
            {
                return;
            }

            box.size = new Vector2(PlayerColliderWidth, PlayerColliderHeight);
            box.offset = new Vector2(0f, PlayerColliderOffsetY);
            EditorUtility.SetDirty(box);
        }

        private static Color Lighten(Color color, float amount)
        {
            return Color.Lerp(color, Color.white, Mathf.Clamp01(amount));
        }

        private static string NormalizeScenePath(string scenePath)
        {
            return string.IsNullOrEmpty(scenePath) ? string.Empty : scenePath.Replace('\\', '/');
        }
    }
}
#endif
