using UnityEngine;

namespace ProjectEclipse.World
{
    [RequireComponent(typeof(Collider2D))]
    public class PlatformSurface : MonoBehaviour
    {
        [SerializeField] private float footlineYOffset;
        [SerializeField] private Color gizmoColor = new Color(0.15f, 0.9f, 0.75f, 0.85f);

        private Collider2D cachedCollider;

        public float SurfaceY
        {
            get
            {
                Collider2D surfaceCollider = GetSurfaceCollider();
                return surfaceCollider != null ? surfaceCollider.bounds.max.y + footlineYOffset : transform.position.y + footlineYOffset;
            }
        }

        public Vector3 GetPositionWithFeetOnSurface(Collider2D standingBody)
        {
            if (standingBody == null)
            {
                return transform.position;
            }

            float feetOffset = standingBody.bounds.min.y - standingBody.transform.position.y;
            Vector3 position = standingBody.transform.position;
            position.y = SurfaceY - feetOffset;
            return position;
        }

        public void SnapFeetToSurface(Collider2D standingBody)
        {
            if (standingBody == null)
            {
                return;
            }

            standingBody.transform.position = GetPositionWithFeetOnSurface(standingBody);
        }

        private Collider2D GetSurfaceCollider()
        {
            if (cachedCollider == null)
            {
                cachedCollider = GetComponent<Collider2D>();
            }

            return cachedCollider;
        }

        private void OnDrawGizmosSelected()
        {
            Collider2D surfaceCollider = GetComponent<Collider2D>();
            if (surfaceCollider == null)
            {
                return;
            }

            Bounds bounds = surfaceCollider.bounds;
            float y = bounds.max.y + footlineYOffset;
            Gizmos.color = gizmoColor;
            Gizmos.DrawLine(new Vector3(bounds.min.x, y, transform.position.z), new Vector3(bounds.max.x, y, transform.position.z));
        }
    }
}
