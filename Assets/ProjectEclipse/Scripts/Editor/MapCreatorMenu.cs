#if UNITY_EDITOR
using ProjectEclipse.Enemies;
using ProjectEclipse.World;
using UnityEditor;
using UnityEngine;

namespace ProjectEclipse.EditorTools
{
    public static class MapCreatorMenu
    {
        private const string PortalPadPath = "Assets/ProjectEclipse/Art/World/portal_pad.png";
        private const string PortalColumnPath = "Assets/ProjectEclipse/Art/World/portal_column.png";
        private const string PortalColumnSheetPath = "Assets/ProjectEclipse/Art/World/portal_column_sheet.png";

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
            CreatePortalVisual(portal.transform, "Teleport Pad", PortalPadPath, new Vector3(0f, -0.48f, 0.02f), new Vector3(1.12f, 0.48f, 1f), new Color(0.55f, 0.9f, 1f, 1f), 1);
            GameObject column = CreatePortalVisual(portal.transform, "Teleport Column", PortalColumnPath, new Vector3(0f, -0.68f, 0.01f), new Vector3(0.78f, 0.96f, 1f), new Color(0.8f, 0.96f, 1f, 1f), 2);
            ConfigurePortalAnimator(column);

            GameObject arrival = new GameObject("Arrival Point");
            arrival.transform.SetParent(portal.transform);
            arrival.transform.localPosition = new Vector3(arrivalOffsetX, 0.16f, 0f);

            RoomPortal2D portalLink = portal.AddComponent<RoomPortal2D>();
            portalLink.Configure(null, arrival.transform, null);
            Register(portal, "Create Portal");
            return portalLink;
        }

        private static GameObject CreatePortalVisual(Transform portal, string name, string spritePath, Vector3 localPosition, Vector3 localScale, Color color, int sortingOrder)
        {
            GameObject visual = new GameObject(name);
            visual.transform.SetParent(portal);
            visual.transform.localPosition = localPosition;
            visual.transform.localScale = localScale;

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return visual;
        }

        private static void ConfigurePortalAnimator(GameObject column)
        {
            if (column == null)
            {
                return;
            }

            PortalVisualAnimator animator = column.GetComponent<PortalVisualAnimator>();
            if (animator == null)
            {
                animator = column.AddComponent<PortalVisualAnimator>();
            }

            Texture2D sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(PortalColumnSheetPath);
            animator.Configure(sheet, 96, 144, 96f, 10f, new Color(0.8f, 0.96f, 1f, 1f));
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
