using UnityEngine;

namespace ProjectEclipse.Utilities
{
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);
        [SerializeField] private float smoothTime = 0.12f;

        private Vector3 velocity;
        private Bounds cameraBounds;
        private bool hasBounds;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            SnapToTarget();
        }

        public void SetBounds(Bounds bounds)
        {
            cameraBounds = bounds;
            hasBounds = true;
        }

        public void ClearBounds()
        {
            hasBounds = false;
        }

        private void Start()
        {
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desired = ClampPosition(target.position + offset);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            transform.position = ClampPosition(transform.position);
        }

        public void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            transform.position = ClampPosition(target.position + offset);
            velocity = Vector3.zero;
        }

        private Vector3 ClampPosition(Vector3 position)
        {
            if (!hasBounds)
            {
                return position;
            }

            Camera camera = GetComponent<Camera>();
            if (camera == null || !camera.orthographic)
            {
                position.x = Mathf.Clamp(position.x, cameraBounds.min.x, cameraBounds.max.x);
                position.y = Mathf.Clamp(position.y, cameraBounds.min.y, cameraBounds.max.y);
                return position;
            }

            float halfHeight = camera.orthographicSize;
            float halfWidth = halfHeight * camera.aspect;
            float minX = cameraBounds.min.x + halfWidth;
            float maxX = cameraBounds.max.x - halfWidth;
            float minY = cameraBounds.min.y + halfHeight;
            float maxY = cameraBounds.max.y - halfHeight;

            position.x = minX <= maxX ? Mathf.Clamp(position.x, minX, maxX) : cameraBounds.center.x;
            position.y = minY <= maxY ? Mathf.Clamp(position.y, minY, maxY) : cameraBounds.center.y;
            return position;
        }
    }
}
