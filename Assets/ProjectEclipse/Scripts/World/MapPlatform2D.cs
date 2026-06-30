using UnityEngine;

namespace ProjectEclipse.World
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(PlatformSurface))]
    public class MapPlatform2D : MonoBehaviour
    {
        [SerializeField] private string platformId = "platform";
        [SerializeField] private float width = 4.5f;
        [SerializeField] private bool oneWay = true;
        [SerializeField] private float colliderHeight = 0.08f;
        [SerializeField] private float solidColliderHeight = 0.28f;

        public string PlatformId { get { return platformId; } }
        public float Width { get { return Mathf.Max(0.25f, width); } }
        public bool OneWay { get { return oneWay; } }

        public void Configure(string id, float platformWidth, bool isOneWay)
        {
            platformId = string.IsNullOrEmpty(id) ? platformId : id;
            width = Mathf.Max(0.25f, platformWidth);
            oneWay = isOneWay;
            SyncComponents(true);
        }

        public void Configure(string id, float platformWidth, bool isOneWay, float height)
        {
            platformId = string.IsNullOrEmpty(id) ? platformId : id;
            width = Mathf.Max(0.25f, platformWidth);
            oneWay = isOneWay;
            if (isOneWay)
            {
                colliderHeight = Mathf.Max(0.02f, height);
            }
            else
            {
                solidColliderHeight = Mathf.Max(0.08f, height);
            }

            SyncComponents(true);
        }

        private void Reset()
        {
            SyncComponents(true);
        }

        private void OnValidate()
        {
            SyncComponents(false);
        }

        private void SyncComponents(bool syncOneWayComponent)
        {
            BoxCollider2D box = GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.size = new Vector2(Width, GetEffectiveColliderHeight());
            }

            if (!syncOneWayComponent)
            {
                return;
            }

            OneWayPlatform oneWayPlatform = GetComponent<OneWayPlatform>();
            if (oneWay && oneWayPlatform == null)
            {
                gameObject.AddComponent<OneWayPlatform>();
            }
            else if (!oneWay && oneWayPlatform != null)
            {
                DestroyComponent(oneWayPlatform);
            }
        }

        private static void DestroyComponent(Component component)
        {
            if (component == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(component);
            }
            else
            {
                Object.DestroyImmediate(component);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = oneWay ? new Color(0.46f, 0.85f, 0.45f, 0.45f) : new Color(0.85f, 0.58f, 0.32f, 0.45f);
            Gizmos.DrawWireCube(transform.position, new Vector3(Width, GetEffectiveColliderHeight(), 0.1f));
        }

        private float GetEffectiveColliderHeight()
        {
            return oneWay ? Mathf.Max(0.02f, colliderHeight) : Mathf.Max(0.08f, solidColliderHeight);
        }
    }
}
