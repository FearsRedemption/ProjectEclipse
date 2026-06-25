using ProjectEclipse.Combat;
using ProjectEclipse.Player;
using ProjectEclipse.Utilities;
using UnityEngine;

namespace ProjectEclipse.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class EnemyProjectile2D : MonoBehaviour
    {
        private int damage = 1;
        private float knockback = 2f;
        private float lifetime = 3f;
        private float spawnedAt;
        private GameObject source;
        private Transform intendedTarget;
        private Rigidbody2D body;

        public void Initialize(GameObject castSource, Transform target, Vector2 direction, int amount, float speed, float knockbackForce, float seconds, Color tint)
        {
            source = castSource;
            intendedTarget = target;
            damage = Mathf.Max(1, amount);
            knockback = Mathf.Max(0f, knockbackForce);
            lifetime = Mathf.Max(0.25f, seconds);
            spawnedAt = Time.time;

            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.linearVelocity = direction.normalized * Mathf.Max(0.5f, speed);

            Collider2D hitbox = GetComponent<Collider2D>();
            hitbox.isTrigger = true;

            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.GetEnemyProjectileSprite();
            renderer.color = Color.Lerp(tint, Color.white, 0.28f);
            renderer.sortingOrder = 10;

            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (Time.time - spawnedAt >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || source == null || other.transform.IsChildOf(source.transform))
            {
                return;
            }

            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player == null || (intendedTarget != null && player.transform != intendedTarget && !player.transform.IsChildOf(intendedTarget)))
            {
                return;
            }

            IDamageable damageable = player.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return;
            }

            Vector2 direction = body != null && body.linearVelocity.sqrMagnitude > 0.0001f
                ? body.linearVelocity.normalized
                : Vector2.right;
            damageable.TakeDamage(new DamageInfo(damage, source, transform.position, direction * knockback + Vector2.up * 0.75f));
            Destroy(gameObject);
        }
    }
}
