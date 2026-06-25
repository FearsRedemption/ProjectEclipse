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

        private struct RoomSpec
        {
            public string Name;
            public Vector2 Center;
            public Vector2 Size;
            public Color Sky;
            public Color Ground;
            public Color Platform;
            public Color Portal;

            public RoomSpec(string name, Vector2 center, Vector2 size, Color sky, Color ground, Color platform, Color portal)
            {
                Name = name;
                Center = center;
                Size = size;
                Sky = sky;
                Ground = ground;
                Platform = platform;
                Portal = portal;
            }
        }

        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private float floorSurfaceY = -2.15f;
        [SerializeField] private Vector2 portalSize = new Vector2(0.9f, 1.55f);

        private readonly List<RoomBounds2D> builtRooms = new List<RoomBounds2D>();
        private readonly List<Transform> builtSpawns = new List<Transform>();
        private PlayerController player;
        private bool built;

        public Vector3 GetSafeRespawnPosition()
        {
            RoomSpec safeRoom = CreateRoomSpecs()[0];
            if (builtRooms.Count > 0 && builtRooms[0] != null)
            {
                Bounds bounds = builtRooms[0].Bounds;
                return new Vector3(bounds.center.x, floorSurfaceY + 0.95f, 0f);
            }

            return new Vector3(safeRoom.Center.x, floorSurfaceY + 0.95f, 0f);
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

            built = true;
            if (player == null)
            {
                player = FindAnyObjectByType<PlayerController>();
            }

            GameObject root = new GameObject(MapRootName);
            RoomSpec[] specs = CreateRoomSpecs();
            for (int i = 0; i < specs.Length; i++)
            {
                BuildRoom(root.transform, specs[i], i);
            }

            for (int i = 0; i < specs.Length - 1; i++)
            {
                RoomPortal2D eastPortal = CreatePortal(root.transform, specs[i], true, builtRooms[i]);
                RoomPortal2D westPortal = CreatePortal(root.transform, specs[i + 1], false, builtRooms[i + 1]);
                eastPortal.LinkTo(westPortal);
                westPortal.LinkTo(eastPortal);
            }

            DistributeExistingEnemies();
            MarkExistingOneWaySurfaces();
            ApplyInitialCameraBounds();
        }

        private static RoomSpec[] CreateRoomSpecs()
        {
            return new RoomSpec[]
            {
                new RoomSpec("Starter Safe Room", new Vector2(120f, 0f), new Vector2(24f, 11.5f), new Color(0.36f, 0.58f, 0.58f), new Color(0.28f, 0.38f, 0.24f), new Color(0.34f, 0.28f, 0.18f), new Color(0.75f, 0.86f, 0.58f)),
                new RoomSpec("Sapling Grove", new Vector2(200f, 0.1f), new Vector2(24f, 11.5f), new Color(0.25f, 0.5f, 0.47f), new Color(0.23f, 0.37f, 0.2f), new Color(0.39f, 0.28f, 0.16f), new Color(0.42f, 0.86f, 0.48f)),
                new RoomSpec("Rock Passage", new Vector2(280f, 0f), new Vector2(24f, 11.5f), new Color(0.31f, 0.39f, 0.43f), new Color(0.31f, 0.31f, 0.32f), new Color(0.43f, 0.43f, 0.43f), new Color(0.66f, 0.74f, 0.86f)),
                new RoomSpec("Crafting Pocket", new Vector2(360f, 0.1f), new Vector2(24f, 11.5f), new Color(0.28f, 0.45f, 0.42f), new Color(0.29f, 0.34f, 0.22f), new Color(0.44f, 0.31f, 0.18f), new Color(0.97f, 0.72f, 0.34f)),
                new RoomSpec("Birchling Canopy", new Vector2(440f, 0.2f), new Vector2(24f, 11.5f), new Color(0.32f, 0.56f, 0.52f), new Color(0.25f, 0.38f, 0.21f), new Color(0.68f, 0.6f, 0.44f), new Color(0.9f, 0.86f, 0.64f)),
                new RoomSpec("Copper Coal Teaser", new Vector2(520f, 0f), new Vector2(24f, 11.5f), new Color(0.29f, 0.33f, 0.36f), new Color(0.23f, 0.22f, 0.21f), new Color(0.55f, 0.36f, 0.22f), new Color(0.95f, 0.55f, 0.28f)),
            };
        }

        private void BuildRoom(Transform root, RoomSpec spec, int index)
        {
            GameObject roomObject = new GameObject(spec.Name + " Bounds");
            roomObject.transform.SetParent(root);
            roomObject.transform.position = new Vector3(spec.Center.x, spec.Center.y, 0f);
            RoomBounds2D bounds = roomObject.AddComponent<RoomBounds2D>();
            bounds.Configure(spec.Size);
            MapArea2D mapArea = roomObject.AddComponent<MapArea2D>();
            mapArea.Configure(spec.Name.ToLowerInvariant().Replace(" ", "-"), spec.Name, spec.Size);
            builtRooms.Add(bounds);

            Transform spawn = CreateSpawn(root, spec, index);
            builtSpawns.Add(spawn);

            CreateSprite(root, spec.Name + " Background", new Vector3(spec.Center.x, spec.Center.y, 2.5f), new Vector3((spec.Size.x + 1.4f) * 0.5f, (spec.Size.y + 1f) * 0.34f, 1f), SpriteFactory.GetRoomBackgroundSprite(), spec.Sky, -40);
            CreateSprite(root, spec.Name + " Ground Fill", new Vector3(spec.Center.x, floorSurfaceY - 1.08f, 1.8f), new Vector3((spec.Size.x + 0.8f) * 0.25f, 1.35f, 1f), SpriteFactory.GetGroundFillSprite(), spec.Ground, -18);
            CreateSolidFloor(root, spec);

            if (index == 1)
            {
                CreateOneWayPlatform(root, spec.Name + " Branch", new Vector2(spec.Center.x - 0.6f, floorSurfaceY + 1.55f), 4.3f, spec.Platform);
            }
            else if (index == 2)
            {
                CreateOneWayPlatform(root, spec.Name + " Stone Shelf", new Vector2(spec.Center.x + 0.4f, floorSurfaceY + 1.25f), 4.8f, spec.Platform);
            }
            else if (index == 4)
            {
                CreateOneWayPlatform(root, spec.Name + " Birch Log", new Vector2(spec.Center.x - 0.6f, floorSurfaceY + 1.7f), 5.2f, spec.Platform);
                CreateOneWayPlatform(root, spec.Name + " Upper Branch", new Vector2(spec.Center.x + 2.2f, floorSurfaceY + 2.85f), 3.6f, spec.Platform);
            }
            else if (index == 5)
            {
                CreateOneWayPlatform(root, spec.Name + " Ore Shelf", new Vector2(spec.Center.x - 0.3f, floorSurfaceY + 1.45f), 4.4f, spec.Platform);
            }
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
            spawn.transform.position = new Vector3(x, floorSurfaceY + 0.95f, 0f);
            return spawn.transform;
        }

        private void CreateSolidFloor(Transform root, RoomSpec spec)
        {
            GameObject floor = new GameObject(spec.Name + " SolidGround");
            floor.transform.SetParent(root);
            floor.transform.position = new Vector3(spec.Center.x, floorSurfaceY - 0.12f, 0f);
            BoxCollider2D collider = floor.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(spec.Size.x + 0.8f, 0.24f);
            floor.AddComponent<PlatformSurface>();
            MapPlatform2D mapPlatform = floor.AddComponent<MapPlatform2D>();
            mapPlatform.Configure(spec.Name.ToLowerInvariant().Replace(" ", "-") + "-floor", spec.Size.x + 0.8f, false);
        }

        private void CreateOneWayPlatform(Transform root, string name, Vector2 center, float width, Color color)
        {
            CreateSprite(root, name + " Art", new Vector3(center.x, center.y - 0.12f, 1.55f), new Vector3(width * 0.25f, 0.4f, 1f), SpriteFactory.GetPlatformStripSprite(), color, -12);

            GameObject surface = new GameObject(name + " Surface");
            surface.transform.SetParent(root);
            surface.transform.position = new Vector3(center.x, center.y, 0f);
            BoxCollider2D collider = surface.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(width, 0.08f);
            surface.AddComponent<OneWayPlatform>();
            surface.AddComponent<PlatformSurface>();
            MapPlatform2D mapPlatform = surface.AddComponent<MapPlatform2D>();
            mapPlatform.Configure(name.ToLowerInvariant().Replace(" ", "-"), width, true);
        }

        private RoomPortal2D CreatePortal(Transform root, RoomSpec spec, bool rightSide, RoomBounds2D owningRoom)
        {
            float x = rightSide ? spec.Center.x + spec.Size.x * 0.5f - 0.85f : spec.Center.x - spec.Size.x * 0.5f + 0.85f;
            GameObject portal = new GameObject(spec.Name + (rightSide ? " East Portal" : " West Portal"));
            portal.transform.SetParent(root);
            portal.transform.position = new Vector3(x, floorSurfaceY + 0.75f, 0f);
            portal.transform.localScale = new Vector3(portalSize.x, portalSize.y, 1f);

            CreateLocalSprite(portal.transform, "Teleport Pad", new Vector3(0f, -0.52f, 0.02f), new Vector3(1.15f, 0.52f, 1f), SpriteFactory.GetPortalPadSprite(), spec.Portal, 1);
            CreateLocalSprite(portal.transform, "Teleport Column", new Vector3(0f, -0.64f, 0.01f), new Vector3(0.72f, 0.92f, 1f), SpriteFactory.GetPortalColumnSprite(), spec.Portal, 2);

            BoxCollider2D trigger = portal.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = Vector2.one;

            GameObject arrival = new GameObject("Arrival Point");
            arrival.transform.SetParent(portal.transform);
            arrival.transform.localPosition = new Vector3(rightSide ? -1.15f : 1.15f, 0.16f, 0f);

            RoomPortal2D portalLink = portal.AddComponent<RoomPortal2D>();
            portalLink.Configure(owningRoom, arrival.transform, null);
            return portalLink;
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

        private static GameObject CreateLocalSprite(Transform root, string name, Vector3 localPosition, Vector3 scale, Sprite sprite, Color color, int sortingOrder)
        {
            GameObject gameObject = CreateSprite(root, name, Vector3.zero, scale, sprite, color, sortingOrder);
            gameObject.transform.localPosition = localPosition;
            return gameObject;
        }

        private void DistributeExistingEnemies()
        {
            EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
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
            if (haystack.Contains("stone") || haystack.Contains("rock"))
            {
                return 2;
            }

            if (haystack.Contains("birch"))
            {
                return 4;
            }

            if (haystack.Contains("copper") || haystack.Contains("coal"))
            {
                return 5;
            }

            return 1;
        }

        private Vector3 GetEnemyPositionInRoom(RoomBounds2D room, int index)
        {
            Bounds bounds = room.Bounds;
            float[] offsets = { -4.4f, -1.4f, 1.8f, 4.8f };
            float offset = offsets[Mathf.Abs(index) % offsets.Length];
            int row = Mathf.Abs(index) / offsets.Length;
            return new Vector3(bounds.center.x + offset, floorSurfaceY + 0.95f + row * 0.05f, 0f);
        }

        private void MarkExistingOneWaySurfaces()
        {
            OneWayPlatform[] oneWayPlatforms = FindObjectsByType<OneWayPlatform>(FindObjectsSortMode.None);
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

            player.transform.position = spawn.position;
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
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

        private static Color Lighten(Color color, float amount)
        {
            return Color.Lerp(color, Color.white, Mathf.Clamp01(amount));
        }
    }
}
