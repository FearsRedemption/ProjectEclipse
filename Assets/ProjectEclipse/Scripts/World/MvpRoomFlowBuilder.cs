using System.Collections.Generic;
using ProjectEclipse.Player;
using ProjectEclipse.Utilities;
using UnityEngine;

namespace ProjectEclipse.World
{
    public class MvpRoomFlowBuilder : MonoBehaviour
    {
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

            GameObject root = new GameObject("Runtime Room Flow");
            RoomSpec[] specs = CreateRoomSpecs();
            for (int i = 0; i < specs.Length; i++)
            {
                BuildRoom(root.transform, specs[i], i);
            }

            for (int i = 0; i < specs.Length - 1; i++)
            {
                CreatePortal(root.transform, specs[i], true, builtRooms[i + 1], builtSpawns[i + 1]);
                CreatePortal(root.transform, specs[i + 1], false, builtRooms[i], builtSpawns[i]);
            }

            MarkExistingOneWaySurfaces();
            ApplyInitialCameraBounds();
        }

        private static RoomSpec[] CreateRoomSpecs()
        {
            return new RoomSpec[]
            {
                new RoomSpec("Starter Safe Room", new Vector2(-17f, 0f), new Vector2(9.5f, 7f), new Color(0.36f, 0.58f, 0.58f), new Color(0.28f, 0.38f, 0.24f), new Color(0.34f, 0.28f, 0.18f), new Color(0.75f, 0.86f, 0.58f)),
                new RoomSpec("Sapling Grove", new Vector2(-7f, 0.1f), new Vector2(10.5f, 7.2f), new Color(0.25f, 0.5f, 0.47f), new Color(0.23f, 0.37f, 0.2f), new Color(0.39f, 0.28f, 0.16f), new Color(0.42f, 0.86f, 0.48f)),
                new RoomSpec("Rock Passage", new Vector2(4f, 0f), new Vector2(10.5f, 7f), new Color(0.31f, 0.39f, 0.43f), new Color(0.31f, 0.31f, 0.32f), new Color(0.43f, 0.43f, 0.43f), new Color(0.66f, 0.74f, 0.86f)),
                new RoomSpec("Crafting Pocket", new Vector2(15f, 0.1f), new Vector2(9f, 7f), new Color(0.28f, 0.45f, 0.42f), new Color(0.29f, 0.34f, 0.22f), new Color(0.44f, 0.31f, 0.18f), new Color(0.97f, 0.72f, 0.34f)),
                new RoomSpec("Birchling Canopy", new Vector2(25f, 0.2f), new Vector2(10.5f, 7.4f), new Color(0.32f, 0.56f, 0.52f), new Color(0.25f, 0.38f, 0.21f), new Color(0.68f, 0.6f, 0.44f), new Color(0.9f, 0.86f, 0.64f)),
                new RoomSpec("Copper Coal Teaser", new Vector2(36f, 0f), new Vector2(10.5f, 7f), new Color(0.29f, 0.33f, 0.36f), new Color(0.23f, 0.22f, 0.21f), new Color(0.55f, 0.36f, 0.22f), new Color(0.95f, 0.55f, 0.28f)),
            };
        }

        private void BuildRoom(Transform root, RoomSpec spec, int index)
        {
            GameObject roomObject = new GameObject(spec.Name + " Bounds");
            roomObject.transform.SetParent(root);
            roomObject.transform.position = new Vector3(spec.Center.x, spec.Center.y, 0f);
            RoomBounds2D bounds = roomObject.AddComponent<RoomBounds2D>();
            bounds.Configure(spec.Size);
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
        }

        private void CreatePortal(Transform root, RoomSpec spec, bool rightSide, RoomBounds2D targetRoom, Transform targetSpawn)
        {
            float x = rightSide ? spec.Center.x + spec.Size.x * 0.5f - 0.85f : spec.Center.x - spec.Size.x * 0.5f + 0.85f;
            GameObject portal = new GameObject(spec.Name + (rightSide ? " East Portal" : " West Portal"));
            portal.transform.SetParent(root);
            portal.transform.position = new Vector3(x, floorSurfaceY + 0.75f, 0f);
            portal.transform.localScale = new Vector3(portalSize.x, portalSize.y, 1f);

            SpriteRenderer renderer = portal.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.GetPortalSprite();
            renderer.color = spec.Portal;
            renderer.sortingOrder = 2;

            BoxCollider2D trigger = portal.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = Vector2.one;

            RoomPortal2D portalLink = portal.AddComponent<RoomPortal2D>();
            portalLink.Configure(targetRoom, targetSpawn);
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
