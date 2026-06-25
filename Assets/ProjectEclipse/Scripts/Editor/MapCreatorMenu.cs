#if UNITY_EDITOR
using ProjectEclipse.Enemies;
using ProjectEclipse.World;
using UnityEditor;
using UnityEngine;

namespace ProjectEclipse.EditorTools
{
    public static class MapCreatorMenu
    {
        [MenuItem("Project Eclipse/Map Creator/Create Area")]
        public static void CreateArea()
        {
            GameObject area = new GameObject("Map Area");
            area.transform.position = GetScenePlacementPosition();
            area.AddComponent<RoomBounds2D>();
            area.AddComponent<MapArea2D>();
            Register(area, "Create Map Area");
        }

        [MenuItem("Project Eclipse/Map Creator/Create One-Way Platform")]
        public static void CreateOneWayPlatform()
        {
            GameObject platform = new GameObject("Editable Platform");
            platform.transform.position = GetScenePlacementPosition();
            platform.AddComponent<BoxCollider2D>();
            platform.AddComponent<PlatformSurface>();
            platform.AddComponent<MapPlatform2D>();
            Register(platform, "Create Map Platform");
        }

        [MenuItem("Project Eclipse/Map Creator/Create Enemy Spawn Point")]
        public static void CreateEnemySpawnPoint()
        {
            GameObject spawn = new GameObject("Enemy Spawn Point");
            spawn.transform.position = GetScenePlacementPosition();
            spawn.AddComponent<EnemySpawnPoint2D>();
            Register(spawn, "Create Enemy Spawn Point");
        }

        [MenuItem("Project Eclipse/Map Creator/Create Linked Portal Pair")]
        public static void CreateLinkedPortalPair()
        {
            Vector3 center = GetScenePlacementPosition();
            RoomPortal2D left = CreatePortal("Portal A", center + Vector3.left * 1.8f, 1.15f);
            RoomPortal2D right = CreatePortal("Portal B", center + Vector3.right * 1.8f, -1.15f);
            left.LinkTo(right);
            right.LinkTo(left);
            Selection.objects = new Object[] { left.gameObject, right.gameObject };
        }

        private static RoomPortal2D CreatePortal(string name, Vector3 position, float arrivalOffsetX)
        {
            GameObject portal = new GameObject(name);
            portal.transform.position = position;
            BoxCollider2D trigger = portal.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(0.9f, 1.55f);

            GameObject arrival = new GameObject("Arrival Point");
            arrival.transform.SetParent(portal.transform);
            arrival.transform.localPosition = new Vector3(arrivalOffsetX, 0.16f, 0f);

            RoomPortal2D portalLink = portal.AddComponent<RoomPortal2D>();
            portalLink.Configure(null, arrival.transform, null);
            Register(portal, "Create Portal");
            return portalLink;
        }

        private static Vector3 GetScenePlacementPosition()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            return sceneView != null ? sceneView.pivot : Vector3.zero;
        }

        private static void Register(GameObject gameObject, string undoName)
        {
            Undo.RegisterCreatedObjectUndo(gameObject, undoName);
            Selection.activeGameObject = gameObject;
        }
    }
}
#endif
