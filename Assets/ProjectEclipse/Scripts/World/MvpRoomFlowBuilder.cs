using System.Collections.Generic;
using ProjectEclipse.Enemies;
using ProjectEclipse.Player;
using ProjectEclipse.Utilities;
using UnityEngine;

namespace ProjectEclipse.World
{
    public class MvpRoomFlowBuilder : MonoBehaviour
    {
        private const string MapRootName = "Editable MVP Map";
        private const float RuntimeRoomWidth = 24f;
        private const float RuntimeRoomHeight = 11.5f;
        private const float HorizontalRoomSpacing = 36f;
        private const float VerticalRoomSpacing = 18f;
        private const float UpperPlatformSurfaceOffset = 3.15f;
        private const float SolidFloorThickness = 0.6f;
        private const float GroundFillVisualHeight = 0.56f;
        private const float OneWayPlatformVisualHeight = 0.24f;
        private const float StandingSurfaceClearance = 0.01f;
        private const float SidePortalInset = 2.8f;
        private const int RouteDepthCount = 5;

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

        private struct EnemySpawnSpec
        {
            public string EnemyId;
            public Vector2 Offset;
            public int MaxAlive;
            public float Radius;
            public float RespawnSeconds;
            public float JitterSeconds;

            public EnemySpawnSpec(string enemyId, Vector2 offset, int maxAlive, float radius, float respawnSeconds, float jitterSeconds)
            {
                EnemyId = enemyId;
                Offset = offset;
                MaxAlive = maxAlive;
                Radius = radius;
                RespawnSeconds = respawnSeconds;
                JitterSeconds = jitterSeconds;
            }
        }

        private struct RoomSpec
        {
            public string Id;
            public string Name;
            public string Route;
            public int Depth;
            public Vector2 Center;
            public Vector2 Size;
            public Color Sky;
            public Color Ground;
            public Color Platform;
            public Color Portal;
            public EnemySpawnSpec[] Spawns;

            public RoomSpec(
                string id,
                string name,
                string route,
                int depth,
                Vector2 center,
                Vector2 size,
                Color sky,
                Color ground,
                Color platform,
                Color portal,
                params EnemySpawnSpec[] spawns)
            {
                Id = id;
                Name = name;
                Route = route;
                Depth = depth;
                Center = center;
                Size = size;
                Sky = sky;
                Ground = ground;
                Platform = platform;
                Portal = portal;
                Spawns = spawns;
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
            public string BaseEnemyId;
            public string MidEnemyId;
            public string HardEnemyId;
            public int DepthCount;
            public string[] DepthNames;

            public RouteSpec(string id, string displayName, int horizontalIndex, Color sky, Color ground, Color platform, Color portal, string baseEnemyId, string midEnemyId, string hardEnemyId, int depthCount, string[] depthNames)
            {
                Id = id;
                DisplayName = displayName;
                HorizontalIndex = horizontalIndex;
                Sky = sky;
                Ground = ground;
                Platform = platform;
                Portal = portal;
                BaseEnemyId = baseEnemyId;
                MidEnemyId = midEnemyId;
                HardEnemyId = hardEnemyId;
                DepthCount = Mathf.Clamp(depthCount, 1, RouteDepthCount);
                DepthNames = depthNames;
            }
        }

        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private float floorSurfaceY = -2.15f;
        [SerializeField] private Vector2 portalSize = new Vector2(0.9f, 1.55f);

        private readonly List<RoomBounds2D> builtRooms = new List<RoomBounds2D>();
        private readonly List<Transform> builtSpawns = new List<Transform>();
        private readonly Dictionary<string, int> roomIndexById = new Dictionary<string, int>();
        private readonly Dictionary<string, EnemyDefinition> enemyDefinitions = new Dictionary<string, EnemyDefinition>();
        private PlayerController player;
        private bool built;

        public Vector3 GetSafeRespawnPosition()
        {
            EnsureRoomCache();
            RoomBounds2D safeRoom = GetRoomById("safe");
            if (safeRoom != null)
            {
                Bounds bounds = safeRoom.Bounds;
                return GetStandingPositionInRoom(safeRoom, bounds.center.x - 1.8f);
            }

            RoomSpec fallbackSafeRoom = CreateRoomSpecs()[0];
            return new Vector3(fallbackSafeRoom.Center.x - 1.8f, floorSurfaceY + GetPlayerFeetOffset() + StandingSurfaceClearance, 0f);
        }

        public void ApplyCameraBoundsForPlayer(Transform playerTransform)
        {
            Camera camera = Camera.main;
            CameraFollow2D follow = camera != null ? camera.GetComponent<CameraFollow2D>() : null;
            if (follow == null || playerTransform == null)
            {
                return;
            }

            follow.SetTarget(playerTransform);
            RoomBounds2D room = FindRoomForPosition(playerTransform.position);
            if (room == null && builtRooms.Count > 0)
            {
                room = builtRooms[0];
            }

            if (room != null)
            {
                follow.SetBounds(room.Bounds);
            }

            follow.SnapToTarget();
        }

        public void Initialize(PlayerController playerController)
        {
            player = playerController;
            if (Application.isPlaying && buildOnStart)
            {
                Build();
                return;
            }

            if (CacheAuthoredRooms() && Application.isPlaying)
            {
                MovePlayerToInitialSpawn();
                ApplyInitialCameraBounds();
            }
        }

        public void ConfigureEnemyDefinitions(IEnumerable<EnemyDefinition> definitions)
        {
            if (definitions == null)
            {
                return;
            }

            foreach (EnemyDefinition definition in definitions)
            {
                RegisterEnemyDefinition(definition);
            }
        }

        private void Start()
        {
            if (buildOnStart)
            {
                Build();
            }
        }

        public void Build()
        {
            if (built)
            {
                return;
            }

            if (player == null)
            {
                player = FindAnyObjectByType<PlayerController>();
            }

            if (CacheAuthoredRooms())
            {
                MovePlayerToInitialSpawn();
                ApplyInitialCameraBounds();
                return;
            }

            built = true;

            GameObject root = new GameObject(MapRootName);
            RoomSpec[] specs = CreateRoomSpecs();
            for (int i = 0; i < specs.Length; i++)
            {
                roomIndexById[specs[i].Id] = i;
                BuildRoom(root.transform, specs[i], i);
            }

            for (int i = 0; i < HorizontalBaseOrder.Length - 1; i++)
            {
                LinkRooms(root.transform, specs, GetDepthRoomId(HorizontalBaseOrder[i], 1), PortalSide.East, GetDepthRoomId(HorizontalBaseOrder[i + 1], 1), PortalSide.West);
            }

            RouteSpec[] routes = CreateRouteSpecs();
            for (int i = 0; i < routes.Length; i++)
            {
                for (int depth = 1; depth < routes[i].DepthCount; depth++)
                {
                    LinkRooms(root.transform, specs, GetDepthRoomId(routes[i].Id, depth), PortalSide.Up, GetDepthRoomId(routes[i].Id, depth + 1), PortalSide.Down);
                }
            }

            DistributeExistingEnemies();
            MarkExistingOneWaySurfaces();
            MovePlayerToInitialSpawn();
            ApplyInitialCameraBounds();
        }

        private static RoomSpec[] CreateRoomSpecs()
        {
            List<RoomSpec> rooms = new List<RoomSpec>
            {
                new RoomSpec("safe", "Safe Zone", "Safe Zone", 0, new Vector2(0f, 0f), new Vector2(RuntimeRoomWidth, RuntimeRoomHeight), new Color(0.36f, 0.58f, 0.58f), new Color(0.28f, 0.38f, 0.24f), new Color(0.34f, 0.28f, 0.18f), new Color(0.75f, 0.86f, 0.58f))
            };

            RouteSpec[] routes = CreateRouteSpecs();
            for (int i = 0; i < routes.Length; i++)
            {
                RouteSpec route = routes[i];
                for (int depth = 1; depth <= route.DepthCount; depth++)
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
                        new Vector2(RuntimeRoomWidth, RuntimeRoomHeight),
                        Color.Lerp(route.Sky, Color.black, depthTint),
                        Color.Lerp(route.Ground, Color.black, depthTint),
                        Color.Lerp(route.Platform, Color.white, depthIndex * 0.035f),
                        Color.Lerp(route.Portal, Color.white, depthIndex * 0.04f),
                        CreateSpawnSpecs(route, depth)));
                }
            }

            return rooms.ToArray();
        }

        private static RouteSpec[] CreateRouteSpecs()
        {
            return new[]
            {
                new RouteSpec("saplings", "Saplings", -1, new Color(0.25f, 0.5f, 0.47f), new Color(0.23f, 0.37f, 0.2f), new Color(0.39f, 0.28f, 0.16f), new Color(0.42f, 0.86f, 0.48f), "sapling", "sapling", "sapling", 1, new[] { "Saplings" }),
                new RouteSpec("birch", "Birch", -2, new Color(0.28f, 0.52f, 0.45f), new Color(0.24f, 0.36f, 0.19f), new Color(0.53f, 0.42f, 0.24f), new Color(0.64f, 0.9f, 0.58f), "birchlet", "birchling", "birchtree", RouteDepthCount, new[] { "Birchlets", "Birchlets + Birchlings", "Birchlings", "Birchlings + Birchtrees", "Birchtree Stand" }),
                new RouteSpec("pine", "Pine", -3, new Color(0.22f, 0.46f, 0.43f), new Color(0.18f, 0.32f, 0.18f), new Color(0.3f, 0.22f, 0.14f), new Color(0.38f, 0.8f, 0.43f), "pinelet", "pineling", "pinetree", RouteDepthCount, new[] { "Pinelets", "Pinelets + Pinelings", "Pinelings", "Pinelings + Pinetrees", "Pinetree Stand" }),
                new RouteSpec("rocks", "Rocks", 1, new Color(0.31f, 0.39f, 0.43f), new Color(0.31f, 0.31f, 0.32f), new Color(0.43f, 0.43f, 0.43f), new Color(0.66f, 0.74f, 0.86f), "rocklet", "rockling", "rock_node", RouteDepthCount, new[] { "Rocklets", "Rocklets + Rocklings", "Rocklings", "Rocklings + Rock Nodes", "Rock Node Cluster" }),
                new RouteSpec("coal", "Coal", 2, new Color(0.26f, 0.3f, 0.34f), new Color(0.21f, 0.21f, 0.2f), new Color(0.36f, 0.35f, 0.34f), new Color(0.84f, 0.62f, 0.42f), "coal_orelet", "coal_oreling", "coal_ore_node", RouteDepthCount, new[] { "Coal Orelets", "Orelets + Orelings", "Coal Orelings", "Orelings + Coal Nodes", "Coal Node Cluster" }),
                new RouteSpec("copper", "Copper", 3, new Color(0.3f, 0.32f, 0.34f), new Color(0.25f, 0.22f, 0.2f), new Color(0.56f, 0.34f, 0.22f), new Color(0.95f, 0.55f, 0.28f), "copper_orelet", "copper_oreling", "copper_ore_node", RouteDepthCount, new[] { "Copper Orelets", "Orelets + Orelings", "Copper Orelings", "Orelings + Ore Nodes", "Copper Ore Nodes" }),
                new RouteSpec("tin", "Tin", 4, new Color(0.32f, 0.39f, 0.43f), new Color(0.28f, 0.28f, 0.3f), new Color(0.48f, 0.5f, 0.5f), new Color(0.78f, 0.86f, 0.9f), "tin_orelet", "tin_oreling", "tin_ore_node", RouteDepthCount, new[] { "Tin Orelets", "Orelets + Orelings", "Tin Orelings", "Orelings + Tin Nodes", "Tin Node Cluster" }),
                new RouteSpec("zync", "Zync", 5, new Color(0.3f, 0.36f, 0.4f), new Color(0.25f, 0.26f, 0.27f), new Color(0.47f, 0.51f, 0.46f), new Color(0.72f, 0.9f, 0.72f), "zync_orelet", "zync_oreling", "zync_ore_node", RouteDepthCount, new[] { "Zync Orelets", "Orelets + Orelings", "Zync Orelings", "Orelings + Zync Nodes", "Zync Node Cluster" }),
                new RouteSpec("miniboss", "Mini Boss", 6, new Color(0.28f, 0.26f, 0.33f), new Color(0.25f, 0.23f, 0.27f), new Color(0.45f, 0.39f, 0.52f), new Color(0.75f, 0.64f, 1f), "route_gate_sentinel", "route_gate_sentinel", "route_gate_sentinel", RouteDepthCount, new[] { "Route Gate", "Gate Approach", "Gate Pressure", "Gate Guards", "Mini Boss Gate" }),
                new RouteSpec("iron", "Iron Ore", 7, new Color(0.27f, 0.31f, 0.34f), new Color(0.23f, 0.23f, 0.24f), new Color(0.42f, 0.4f, 0.39f), new Color(0.74f, 0.76f, 0.8f), "iron_orelet", "iron_oreling", "iron_ore_node", RouteDepthCount, new[] { "Iron Orelets", "Orelets + Orelings", "Iron Orelings", "Orelings + Iron Nodes", "Iron Node Cluster" })
            };
        }

        private static EnemySpawnSpec[] CreateSpawnSpecs(RouteSpec route, int depth)
        {
            switch (depth)
            {
                case 1:
                    return new[] { new EnemySpawnSpec(route.BaseEnemyId, new Vector2(-2.8f, 0f), 4, 3.4f, 4.2f, 1.1f) };
                case 2:
                    return new[]
                    {
                        new EnemySpawnSpec(route.BaseEnemyId, new Vector2(-4.2f, 0f), 3, 3.2f, 4.1f, 1f),
                        new EnemySpawnSpec(route.MidEnemyId, new Vector2(3.1f, 0f), 2, 2.8f, 5.2f, 1.2f)
                    };
                case 3:
                    return new[] { new EnemySpawnSpec(route.MidEnemyId, new Vector2(-1.2f, 0f), 5, 4.4f, 5.4f, 1.4f) };
                case 4:
                    return new[]
                    {
                        new EnemySpawnSpec(route.MidEnemyId, new Vector2(-4.1f, 0f), 3, 3.4f, 5.6f, 1.3f),
                        new EnemySpawnSpec(route.HardEnemyId, new Vector2(3.7f, 0f), 2, 3f, 7.2f, 1.5f)
                    };
                case 5:
                    return new[] { new EnemySpawnSpec(route.HardEnemyId, new Vector2(-1f, 0f), 5, 4.6f, 7.8f, 1.7f) };
                default:
                    return null;
            }
        }

        private static string GetDepthRoomId(string routeId, int depth)
        {
            return routeId == "safe" ? "safe" : routeId + "-d" + depth.ToString();
        }

        private void BuildRoom(Transform root, RoomSpec spec, int index)
        {
            GameObject roomObject = new GameObject(spec.Name + " Bounds");
            roomObject.transform.SetParent(root);
            roomObject.transform.position = new Vector3(spec.Center.x, spec.Center.y, 0f);
            RoomBounds2D bounds = roomObject.AddComponent<RoomBounds2D>();
            bounds.Configure(spec.Size);
            MapArea2D mapArea = roomObject.AddComponent<MapArea2D>();
            mapArea.Configure(spec.Id, spec.Name, spec.Size);
            builtRooms.Add(bounds);

            Transform spawn = CreateSpawn(root, spec, index);
            builtSpawns.Add(spawn);

            CreateSprite(root, spec.Name + " Background", new Vector3(spec.Center.x, spec.Center.y, 2.5f), new Vector3((spec.Size.x + 1.4f) * 0.5f, (spec.Size.y + 1f) * 0.34f, 1f), SpriteFactory.GetRoomBackgroundSprite(), spec.Sky, -40);
            CreateTiledSprite(root, spec.Name + " Ground Fill", new Vector3(spec.Center.x, spec.Center.y + floorSurfaceY - GroundFillVisualHeight * 0.5f, 1.8f), new Vector2(spec.Size.x + 0.8f, GroundFillVisualHeight), SpriteFactory.GetGroundFillSprite(), spec.Ground, -18);
            CreateSolidFloor(root, spec);

            CreateRoomPlatforms(root, spec);
            CreateEnemySpawnPoints(root, spec);
        }

        private Transform CreateSpawn(Transform root, RoomSpec spec, int index)
        {
            GameObject spawn = new GameObject(spec.Name + " Spawn");
            spawn.transform.SetParent(root);
            float x = spec.Center.x - spec.Size.x * 0.5f + 1.2f;
            if (index == 0)
            {
                x = spec.Center.x - 1.8f;
            }
            spawn.transform.position = new Vector3(x, spec.Center.y + floorSurfaceY + GetPlayerFeetOffset() + StandingSurfaceClearance, 0f);
            return spawn.transform;
        }

        private void CreateRoomPlatforms(Transform root, RoomSpec spec)
        {
            if (spec.Id == "safe")
            {
                return;
            }

            CreateOneWayPlatform(root, spec.Name + " Lower Step", new Vector2(spec.Center.x - 3.6f, spec.Center.y + floorSurfaceY + 1.05f), 4.2f, spec.Platform);
            CreateOneWayPlatform(root, spec.Name + " Mid Step", new Vector2(spec.Center.x + 1.2f, spec.Center.y + floorSurfaceY + 2.05f), 4.2f, Lighten(spec.Platform, 0.08f));
            CreateOneWayPlatform(root, spec.Name + " Upper Step", new Vector2(spec.Center.x + spec.Size.x * 0.32f, spec.Center.y + GetUpperPlatformSurfaceY()), 3.8f, Lighten(spec.Platform, 0.16f));
        }

        private void CreateEnemySpawnPoints(Transform root, RoomSpec spec)
        {
            if (spec.Spawns == null)
            {
                return;
            }

            for (int i = 0; i < spec.Spawns.Length; i++)
            {
                EnemySpawnSpec spawnSpec = spec.Spawns[i];
                EnemyDefinition definition = FindEnemyDefinition(spawnSpec.EnemyId);
                if (definition == null)
                {
                    continue;
                }

                GameObject spawn = new GameObject(spec.Name + " " + definition.DisplayName + " Spawn");
                spawn.transform.SetParent(root);
                spawn.transform.position = new Vector3(spec.Center.x + spawnSpec.Offset.x, spec.Center.y + floorSurfaceY + GetEnemyFeetOffset(definition) + spawnSpec.Offset.y + StandingSurfaceClearance, 0f);
                EnemySpawnPoint2D spawnPoint = spawn.AddComponent<EnemySpawnPoint2D>();
                spawnPoint.Configure(definition, spawnSpec.MaxAlive, spawnSpec.Radius, spawnSpec.RespawnSeconds, spawnSpec.JitterSeconds);
            }
        }

        private void CreateSolidFloor(Transform root, RoomSpec spec)
        {
            GameObject floor = new GameObject(spec.Name + " SolidGround");
            floor.transform.SetParent(root);
            floor.transform.position = new Vector3(spec.Center.x, spec.Center.y + floorSurfaceY - SolidFloorThickness * 0.5f, 0f);
            BoxCollider2D collider = floor.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(spec.Size.x + 0.8f, SolidFloorThickness);
            floor.AddComponent<PlatformSurface>();
            MapPlatform2D mapPlatform = floor.AddComponent<MapPlatform2D>();
            mapPlatform.Configure(spec.Id + "-floor", spec.Size.x + 0.8f, false, SolidFloorThickness);
        }

        private void CreateOneWayPlatform(Transform root, string name, Vector2 center, float width, Color color)
        {
            CreateTiledSprite(root, name + " Art", new Vector3(center.x, center.y + 0.04f - OneWayPlatformVisualHeight * 0.5f, 1.55f), new Vector2(width, OneWayPlatformVisualHeight), SpriteFactory.GetPlatformStripSprite(), color, -12);

            GameObject surface = new GameObject(name + " Surface");
            surface.transform.SetParent(root);
            surface.transform.position = new Vector3(center.x, center.y, 0f);
            BoxCollider2D collider = surface.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(width, 0.08f);
            surface.AddComponent<OneWayPlatform>();
            surface.AddComponent<PlatformSurface>();
            MapPlatform2D mapPlatform = surface.AddComponent<MapPlatform2D>();
            mapPlatform.Configure(name.ToLowerInvariant().Replace(" ", "-"), width, true, 0.08f);
        }

        private RoomPortal2D CreatePortal(Transform root, RoomSpec spec, PortalSide side, RoomBounds2D owningRoom)
        {
            Vector2 portalPosition = GetPortalPosition(spec, side);
            GameObject portal = new GameObject(spec.Name + " " + side + " Portal");
            portal.transform.SetParent(root);
            portal.transform.position = new Vector3(portalPosition.x, portalPosition.y, 0f);
            portal.transform.localScale = new Vector3(portalSize.x, portalSize.y, 1f);

            CreateLocalSprite(portal.transform, "Teleport Pad", new Vector3(0f, -0.52f, 0.02f), new Vector3(1.15f, 0.52f, 1f), SpriteFactory.GetPortalPadSprite(), spec.Portal, 1);
            CreateLocalSprite(portal.transform, "Teleport Column", new Vector3(0f, -0.64f, 0.01f), new Vector3(0.72f, 0.92f, 1f), SpriteFactory.GetPortalColumnSprite(), spec.Portal, 2);

            BoxCollider2D trigger = portal.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = Vector2.one;

            GameObject arrival = new GameObject("Arrival Point");
            arrival.transform.SetParent(portal.transform);
            arrival.transform.position = GetArrivalPosition(spec, side);

            RoomPortal2D portalLink = portal.AddComponent<RoomPortal2D>();
            portalLink.Configure(owningRoom, arrival.transform, null);
            return portalLink;
        }

        private void LinkRooms(Transform root, RoomSpec[] specs, string fromId, PortalSide fromSide, string toId, PortalSide toSide)
        {
            int fromIndex;
            int toIndex;
            if (!roomIndexById.TryGetValue(fromId, out fromIndex) || !roomIndexById.TryGetValue(toId, out toIndex))
            {
                return;
            }

            RoomPortal2D first = CreatePortal(root, specs[fromIndex], fromSide, builtRooms[fromIndex]);
            RoomPortal2D second = CreatePortal(root, specs[toIndex], toSide, builtRooms[toIndex]);
            first.LinkTo(second);
            second.LinkTo(first);
        }

        private Vector2 GetPortalPosition(RoomSpec spec, PortalSide side)
        {
            float x;
            switch (side)
            {
                case PortalSide.West:
                    x = spec.Center.x - spec.Size.x * 0.5f + SidePortalInset;
                    break;
                case PortalSide.East:
                    x = spec.Center.x + spec.Size.x * 0.5f - SidePortalInset;
                    break;
                case PortalSide.Up:
                    x = spec.Center.x + spec.Size.x * 0.32f;
                    break;
                case PortalSide.Down:
                    x = spec.Center.x - spec.Size.x * 0.32f;
                    break;
                default:
                    x = spec.Center.x;
                    break;
            }

            float surfaceY = side == PortalSide.Up ? GetUpperPlatformSurfaceY() : floorSurfaceY;
            return new Vector2(x, spec.Center.y + surfaceY + 0.75f);
        }

        private float GetUpperPlatformSurfaceY()
        {
            return floorSurfaceY + UpperPlatformSurfaceOffset;
        }

        private Vector3 GetArrivalPosition(RoomSpec spec, PortalSide side)
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

            float surfaceY = side == PortalSide.Up ? GetUpperPlatformSurfaceY() : floorSurfaceY;
            return new Vector3(portal.x + xOffset, spec.Center.y + surfaceY + GetPlayerFeetOffset() + StandingSurfaceClearance, 0f);
        }

        private static GameObject CreateSprite(Transform root, string name, Vector3 position, Vector3 scale, Sprite sprite, Color color, int sortingOrder)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.transform.SetParent(root);
            gameObject.transform.position = position;
            gameObject.transform.localScale = scale;

            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return gameObject;
        }

        private static GameObject CreateTiledSprite(Transform root, string name, Vector3 position, Vector2 size, Sprite sprite, Color color, int sortingOrder)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.transform.SetParent(root);
            gameObject.transform.position = position;
            gameObject.transform.localScale = Vector3.one;

            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = size;
            return gameObject;
        }

        private static GameObject CreateLocalSprite(Transform root, string name, Vector3 localPosition, Vector3 scale, Sprite sprite, Color color, int sortingOrder)
        {
            GameObject gameObject = CreateSprite(root, name, Vector3.zero, scale, sprite, color, sortingOrder);
            gameObject.transform.localPosition = localPosition;
            return gameObject;
        }

        private void DistributeExistingEnemies()
        {
            EnemyController[] enemies = FindObjectsByType<EnemyController>();
            Dictionary<int, int> roomCounts = new Dictionary<int, int>();
            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null)
                {
                    continue;
                }

                int roomIndex = GetRoomIndexForEnemy(enemy);
                if (roomIndex < 0 || roomIndex >= builtRooms.Count || builtRooms[roomIndex] == null)
                {
                    continue;
                }

                int count;
                roomCounts.TryGetValue(roomIndex, out count);
                roomCounts[roomIndex] = count + 1;
                enemy.transform.position = GetEnemyPositionInRoom(builtRooms[roomIndex], count);
            }
        }

        private int GetRoomIndexForEnemy(EnemyController enemy)
        {
            string id = enemy != null && enemy.Definition != null ? enemy.Definition.EnemyId.ToLowerInvariant() : string.Empty;
            string label = enemy != null && enemy.Definition != null ? enemy.Definition.DisplayName.ToLowerInvariant() : string.Empty;
            string haystack = id + " " + label;
            if (haystack.Contains("birchtree"))
            {
                return GetRoomIndexOrDefault("birch-d5", 0);
            }

            if (haystack.Contains("birchling"))
            {
                return GetRoomIndexOrDefault("birch-d3", 0);
            }

            if (haystack.Contains("birchlet") || haystack.Contains("birch"))
            {
                return GetRoomIndexOrDefault("birch-d1", 0);
            }

            if (haystack.Contains("pinetree"))
            {
                return GetRoomIndexOrDefault("pine-d5", 0);
            }

            if (haystack.Contains("pineling"))
            {
                return GetRoomIndexOrDefault("pine-d3", 0);
            }

            if (haystack.Contains("pinelet") || haystack.Contains("pine"))
            {
                return GetRoomIndexOrDefault("pine-d1", 0);
            }

            if (haystack.Contains("sapling") || haystack.Contains("tree"))
            {
                return GetRoomIndexOrDefault("saplings-d1", 0);
            }

            if (haystack.Contains("route_gate") || haystack.Contains("sentinel") || haystack.Contains("boss"))
            {
                return GetRoomIndexOrDefault("miniboss-d5", 0);
            }

            if (haystack.Contains("tin"))
            {
                return GetRoomIndexOrDefault(GetMaterialDepthRoom("tin", haystack), 0);
            }

            if (haystack.Contains("zync") || haystack.Contains("zinc"))
            {
                return GetRoomIndexOrDefault(GetMaterialDepthRoom("zync", haystack), 0);
            }

            if (haystack.Contains("iron"))
            {
                return GetRoomIndexOrDefault(GetMaterialDepthRoom("iron", haystack), 0);
            }

            if (haystack.Contains("copper"))
            {
                return GetRoomIndexOrDefault(GetMaterialDepthRoom("copper", haystack), 0);
            }

            if (haystack.Contains("stone") || haystack.Contains("rock"))
            {
                return GetRoomIndexOrDefault(GetMaterialDepthRoom("rocks", haystack), 0);
            }

            if (haystack.Contains("coal"))
            {
                return GetRoomIndexOrDefault(GetMaterialDepthRoom("coal", haystack), 0);
            }

            return GetRoomIndexOrDefault("saplings-d1", 0);
        }

        private static string GetMaterialDepthRoom(string routeId, string haystack)
        {
            if (haystack.Contains("node"))
            {
                return routeId + "-d5";
            }

            if (haystack.Contains("ling"))
            {
                return routeId + "-d3";
            }

            return routeId + "-d1";
        }

        private int GetRoomIndexOrDefault(string id, int fallback)
        {
            int index;
            return roomIndexById.TryGetValue(id, out index) ? index : fallback;
        }

        private Vector3 GetEnemyPositionInRoom(RoomBounds2D room, int index)
        {
            Bounds bounds = room.Bounds;
            float[] offsets = { -4.4f, -1.4f, 1.8f, 4.8f };
            float offset = offsets[Mathf.Abs(index) % offsets.Length];
            int row = Mathf.Abs(index) / offsets.Length;
            return new Vector3(bounds.center.x + offset, bounds.center.y + floorSurfaceY + 0.95f + row * 0.05f, 0f);
        }

        private void MarkExistingOneWaySurfaces()
        {
            OneWayPlatform[] oneWayPlatforms = FindObjectsByType<OneWayPlatform>();
            for (int i = 0; i < oneWayPlatforms.Length; i++)
            {
                if (oneWayPlatforms[i] != null && oneWayPlatforms[i].GetComponent<PlatformSurface>() == null)
                {
                    oneWayPlatforms[i].gameObject.AddComponent<PlatformSurface>();
                }
            }
        }

        private void ApplyInitialCameraBounds()
        {
            Camera camera = Camera.main;
            CameraFollow2D follow = camera != null ? camera.GetComponent<CameraFollow2D>() : null;
            if (follow == null || player == null)
            {
                return;
            }

            follow.SetTarget(player.transform);
            RoomBounds2D room = FindRoomForPosition(player.transform.position);
            if (room == null && builtSpawns.Count > 0 && builtSpawns[0] != null)
            {
                MovePlayerToSpawn(builtSpawns[0]);
                room = builtRooms.Count > 0 ? builtRooms[0] : null;
            }

            if (room == null && builtRooms.Count > 0)
            {
                room = builtRooms[0];
            }

            if (room != null)
            {
                follow.SetBounds(room.Bounds);
                follow.SnapToTarget();
            }
        }

        private void MovePlayerToSpawn(Transform spawn)
        {
            if (player == null || spawn == null)
            {
                return;
            }

            RoomBounds2D room = FindRoomForPosition(spawn.position);
            player.transform.position = room != null
                ? GetStandingPositionInRoom(room, spawn.position.x)
                : spawn.position;
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }

        private void MovePlayerToInitialSpawn()
        {
            if (builtSpawns.Count == 0)
            {
                return;
            }

            int safeIndex;
            Transform spawn = roomIndexById.TryGetValue("safe", out safeIndex) && safeIndex >= 0 && safeIndex < builtSpawns.Count
                ? builtSpawns[safeIndex]
                : builtSpawns[0];
            MovePlayerToSpawn(spawn);
        }

        private RoomBounds2D FindRoomForPosition(Vector3 position)
        {
            for (int i = 0; i < builtRooms.Count; i++)
            {
                if (builtRooms[i] != null && builtRooms[i].Contains(position))
                {
                    return builtRooms[i];
                }
            }

            return null;
        }

        private void RegisterEnemyDefinition(EnemyDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            string id = definition.EnemyId.ToLowerInvariant();
            string label = definition.DisplayName.ToLowerInvariant();
            if (!enemyDefinitions.ContainsKey(id))
            {
                enemyDefinitions.Add(id, definition);
            }

            if (!enemyDefinitions.ContainsKey(label))
            {
                enemyDefinitions.Add(label, definition);
            }
        }

        private EnemyDefinition FindEnemyDefinition(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            EnemyDefinition definition;
            return enemyDefinitions.TryGetValue(id.ToLowerInvariant(), out definition) ? definition : null;
        }

        private void EnsureRoomCache()
        {
            if (builtRooms.Count > 0)
            {
                return;
            }

            CacheAuthoredRooms();
        }

        private bool CacheAuthoredRooms()
        {
            if (builtRooms.Count > 0)
            {
                return true;
            }

            MapArea2D[] areas = FindObjectsByType<MapArea2D>();
            if (areas == null || areas.Length == 0)
            {
                return false;
            }

            builtRooms.Clear();
            builtSpawns.Clear();
            roomIndexById.Clear();

            System.Array.Sort(areas, CompareMapAreas);
            for (int i = 0; i < areas.Length; i++)
            {
                MapArea2D area = areas[i];
                if (area == null || area.RoomBounds == null)
                {
                    continue;
                }

                string id = area.AreaId.ToLowerInvariant();
                if (roomIndexById.ContainsKey(id))
                {
                    continue;
                }

                roomIndexById[id] = builtRooms.Count;
                builtRooms.Add(area.RoomBounds);
                builtSpawns.Add(FindAuthoredSpawnForRoom(area));
            }

            built = builtRooms.Count > 0;
            return built;
        }

        private static int CompareMapAreas(MapArea2D left, MapArea2D right)
        {
            int leftPriority = GetAreaSortPriority(left);
            int rightPriority = GetAreaSortPriority(right);
            if (leftPriority != rightPriority)
            {
                return leftPriority.CompareTo(rightPriority);
            }

            Vector3 leftPosition = left != null ? left.transform.position : Vector3.zero;
            Vector3 rightPosition = right != null ? right.transform.position : Vector3.zero;
            int yCompare = rightPosition.y.CompareTo(leftPosition.y);
            return yCompare != 0 ? yCompare : leftPosition.x.CompareTo(rightPosition.x);
        }

        private static int GetAreaSortPriority(MapArea2D area)
        {
            if (area == null)
            {
                return 999;
            }

            string id = area.AreaId.ToLowerInvariant();
            if (id == "safe")
            {
                return 0;
            }

            string routeId = GetRouteId(id);
            for (int i = 0; i < HorizontalBaseOrder.Length; i++)
            {
                if (HorizontalBaseOrder[i] == routeId)
                {
                    return 10 + i * 10 + GetDepthSort(area);
                }
            }

            return 100;
        }

        private static int GetDepthSort(MapArea2D area)
        {
            if (area == null)
            {
                return 0;
            }

            string id = area.AreaId.ToLowerInvariant();
            return GetDepthNumber(id);
        }

        private static string GetRouteId(string roomId)
        {
            int depthIndex = roomId.LastIndexOf("-d");
            return depthIndex > 0 ? roomId.Substring(0, depthIndex) : roomId;
        }

        private static int GetDepthNumber(string roomId)
        {
            int depthIndex = roomId.LastIndexOf("-d");
            if (depthIndex < 0 || depthIndex + 2 >= roomId.Length)
            {
                return 1;
            }

            int depth;
            return int.TryParse(roomId.Substring(depthIndex + 2), out depth) ? Mathf.Clamp(depth, 1, RouteDepthCount) : 1;
        }

        private Transform FindAuthoredSpawnForRoom(MapArea2D area)
        {
            if (area == null || area.RoomBounds == null)
            {
                return null;
            }

            Transform[] transforms = FindObjectsByType<Transform>();
            string id = area.AreaId.ToLowerInvariant();
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null)
                {
                    continue;
                }

                string name = candidate.name.ToLowerInvariant();
                if (name.Contains("spawn") && name.Contains(id) && area.RoomBounds.Contains(candidate.position))
                {
                    return candidate;
                }
            }

            return null;
        }

        private RoomBounds2D GetRoomById(string id)
        {
            EnsureRoomCache();
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            int index;
            return roomIndexById.TryGetValue(id.ToLowerInvariant(), out index) && index >= 0 && index < builtRooms.Count
                ? builtRooms[index]
                : null;
        }

        private Vector3 GetStandingPositionInRoom(RoomBounds2D room, float desiredX)
        {
            if (room == null)
            {
                return new Vector3(desiredX, floorSurfaceY + GetPlayerFeetOffset() + StandingSurfaceClearance, 0f);
            }

            Bounds bounds = room.Bounds;
            float clampedX = Mathf.Clamp(desiredX, bounds.min.x + 1f, bounds.max.x - 1f);
            PlatformSurface surface = FindBestStandingSurface(room, clampedX);
            float surfaceY = surface != null ? surface.SurfaceY : room.Bounds.center.y + floorSurfaceY;
            return new Vector3(clampedX, surfaceY + GetPlayerFeetOffset() + StandingSurfaceClearance, 0f);
        }

        private PlatformSurface FindBestStandingSurface(RoomBounds2D room, float x)
        {
            PlatformSurface[] surfaces = FindObjectsByType<PlatformSurface>();
            PlatformSurface best = null;
            float bestY = float.NegativeInfinity;
            for (int i = 0; i < surfaces.Length; i++)
            {
                PlatformSurface surface = surfaces[i];
                if (surface == null || surface.GetComponent<OneWayPlatform>() != null)
                {
                    continue;
                }

                Collider2D surfaceCollider = surface.GetComponent<Collider2D>();
                if (surfaceCollider == null)
                {
                    continue;
                }

                Bounds surfaceBounds = surfaceCollider.bounds;
                if (x < surfaceBounds.min.x || x > surfaceBounds.max.x || !room.Contains(surface.transform.position))
                {
                    continue;
                }

                float y = surface.SurfaceY;
                if (y > bestY)
                {
                    best = surface;
                    bestY = y;
                }
            }

            return best;
        }

        private float GetPlayerFeetOffset()
        {
            if (player == null)
            {
                player = FindAnyObjectByType<PlayerController>();
            }

            if (player != null)
            {
                player.NormalizeColliderForVisualFooting();
            }

            Collider2D collider = player != null ? player.GetComponent<Collider2D>() : null;
            if (collider == null)
            {
                return 0.72f;
            }

            return Mathf.Max(0.05f, player.transform.position.y - collider.bounds.min.y);
        }

        private static float GetEnemyFeetOffset(EnemyDefinition definition)
        {
            if (definition == null)
            {
                return 0.55f;
            }

            return Mathf.Max(0.15f, definition.ColliderSize.y * Mathf.Abs(definition.VisualScale.y) * 0.5f);
        }

        private static Color Lighten(Color color, float amount)
        {
            return Color.Lerp(color, Color.white, Mathf.Clamp01(amount));
        }
    }
}
