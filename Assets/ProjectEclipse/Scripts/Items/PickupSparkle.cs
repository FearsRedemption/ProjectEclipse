using ProjectEclipse.Utilities;
using UnityEngine;

namespace ProjectEclipse.Items
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PickupSparkle : MonoBehaviour
    {
        [SerializeField] private float duration = 0.32f;

        private SpriteRenderer spriteRenderer;
        private Color startColor = Color.white;
        private Vector3 startScale = Vector3.one;
        private float startedAt;

        public static void Spawn(Vector3 position, Color color)
        {
            GameObject sparkle = new GameObject("Pickup Sparkle");
            sparkle.transform.position = position;
            sparkle.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

            SpriteRenderer renderer = sparkle.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.GetSparkleSprite();
            renderer.color = color;
            renderer.sortingOrder = 14;

            PickupSparkle effect = sparkle.AddComponent<PickupSparkle>();
            effect.Initialize(color, 0.32f);
        }

        public void Initialize(Color color, float lifetime)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            startColor = color;
            duration = Mathf.Max(0.05f, lifetime);
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
            transform.localScale = startScale * (1f + t * 0.8f);
            transform.position += Vector3.up * (Time.deltaTime * 0.45f);

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
