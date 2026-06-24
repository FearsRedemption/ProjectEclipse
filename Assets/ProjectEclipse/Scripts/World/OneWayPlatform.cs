using UnityEngine;

namespace ProjectEclipse.World
{
    [RequireComponent(typeof(Collider2D))]
    public class OneWayPlatform : MonoBehaviour
    {
        [SerializeField] private float surfaceTolerance = 0.08f;

        public float SurfaceTolerance { get { return Mathf.Max(0f, surfaceTolerance); } }
    }
}
