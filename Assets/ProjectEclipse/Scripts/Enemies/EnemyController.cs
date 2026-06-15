using System.Collections;
using ProjectEclipse.Combat;
using ProjectEclipse.Items;
using ProjectEclipse.Utilities;
using UnityEngine;

namespace ProjectEclipse.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class EnemyController : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyDefinition definition;

        private Transform target;
        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private VisualStateAnimator visualState;
        private DropSpawner dropSpawner;
        private int currentHealth;
        private float nextAttackTime;
        private int facingDirection = -1;
        private bool dead;

        public bool IsAlive { get { return !dead; } }
        public EnemyDefinition Definition { get { return definition; } }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            visualState = GetComponent<VisualStateAnimator>();
        }

        public void Initialize(EnemyDefinition enemyDefinition, Transform playerTarget, DropSpawner spawner)
        {
            definition = enemyDefinition;
            target = playerTarget;
            dropSpawner = spawner;
            currentHealth = definition != null ? definition.MaxHealth : 1;
            if (spriteRenderer != null && definition != null)
            {
                spriteRenderer.color = definition.PlaceholderColor;
            }
        }

        private void Update()
        {
            if (dead || definition == null || target == null)
            {
                return;
            }

            Vector2 toTarget = (Vector2)(target.position - transform.position);
            float distance = Mathf.Abs(toTarget.x);

            if (distance <= definition.AttackRange && Mathf.Abs(toTarget.y) < 1.3f)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                AttackIfReady();
                if (visualState != null)
                {
                    visualState.SetMoving(false);
                }
                return;
            }

            if (distance <= definition.ChaseRange)
            {
                facingDirection = toTarget.x >= 0f ? 1 : -1;
                body.linearVelocity = new Vector2(facingDirection * definition.MoveSpeed, body.linearVelocity.y);
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * facingDirection;
                transform.localScale = scale;
                if (visualState != null)
                {
                    visualState.SetMoving(true);
                }
            }
            else if (visualState != null)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                visualState.SetMoving(false);
            }
        }

        public void TakeDamage(DamageInfo damage)
        {
            if (dead || definition == null)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - damage.Amount);
            body.AddForce(damage.Knockback, ForceMode2D.Impulse);
            if (visualState != null)
            {
                visualState.TriggerHurt();
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void AttackIfReady()
        {
            if (Time.time < nextAttackTime)
            {
                return;
            }

            nextAttackTime = Time.time + definition.AttackCooldown;
            if (visualState != null)
            {
                visualState.TriggerAttack();
            }

            IDamageable damageable = target.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                Vector2 knockback = new Vector2(facingDirection * 2.4f, 1.2f);
                damageable.TakeDamage(new DamageInfo(definition.ContactDamage, gameObject, (Vector2)transform.position, knockback));
            }
        }

        private void Die()
        {
            dead = true;
            body.linearVelocity = Vector2.zero;
            Collider2D ownCollider = GetComponent<Collider2D>();
            if (ownCollider != null)
            {
                ownCollider.enabled = false;
            }

            if (visualState != null)
            {
                visualState.TriggerDie();
            }

            SpawnDrops();
            StartCoroutine(DestroyAfterDeath());
        }

        private void SpawnDrops()
        {
            if (dropSpawner == null || definition == null)
            {
                return;
            }

            for (int i = 0; i < definition.Drops.Count; i++)
            {
                DropTableEntry drop = definition.Drops[i];
                if (drop.Item == null || UnityEngine.Random.value > drop.Chance)
                {
                    continue;
                }

                int quantity = UnityEngine.Random.Range(drop.MinQuantity, drop.MaxQuantity + 1);
                dropSpawner.SpawnDrop(drop.Item, quantity, transform.position + Vector3.up * 0.35f);
            }
        }

        private IEnumerator DestroyAfterDeath()
        {
            yield return new WaitForSeconds(0.45f);
            Destroy(gameObject);
        }
    }
}
