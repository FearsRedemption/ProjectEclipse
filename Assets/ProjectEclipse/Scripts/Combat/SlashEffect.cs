using UnityEngine;

namespace ProjectEclipse.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SlashEffect : MonoBehaviour
    {
        [SerializeField] private float duration = 0.14f;
        [SerializeField] private float scaleGrowth = 0.22f;

        private SpriteRenderer spriteRenderer;
        private Color startColor = Color.white;
        private Vector3 startScale = Vector3.one;
        private float startedAt;

        public void Initialize(Color color, float lifetime)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            startColor = color;
            duration = Mathf.Max(0.04f, lifetime);
            startedAt = Time.time;
            startScale = transform.localScale;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = startColor;
            }
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            startedAt = Time.time;
            startScale = transform.localScale;
            if (spriteRenderer != null)
            {
                startColor = spriteRenderer.color;
            }
        }

        private void Update()
        {
            float t = Mathf.Clamp01((Time.time - startedAt) / Mathf.Max(0.01f, duration));
            transform.localScale = startScale * (1f + t * scaleGrowth);
            if (spriteRenderer != null)
            {
                Color color = startColor;
                color.a = startColor.a * (1f - t);
                spriteRenderer.color = color;
            }

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
