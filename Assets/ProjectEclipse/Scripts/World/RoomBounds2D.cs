using UnityEngine;

namespace ProjectEclipse.World
{
    public class RoomBounds2D : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new Vector2(12f, 7f);
        [SerializeField] private Color gizmoColor = new Color(0.25f, 0.7f, 1f, 0.45f);

        public Bounds Bounds
        {
            get { return new Bounds(transform.position, new Vector3(Mathf.Max(1f, size.x), Mathf.Max(1f, size.y), 1f)); }
        }

        public Vector2 Size { get { return size; } }

        public void Configure(Vector2 newSize)
        {
            size = new Vector2(Mathf.Max(1f, newSize.x), Mathf.Max(1f, newSize.y));
        }

        public bool Contains(Vector3 position)
        {
            return Bounds.Contains(new Vector3(position.x, position.y, transform.position.z));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            Bounds bounds = Bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}
