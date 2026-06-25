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
        [SerializeField] private LayerMask platformMask = ~0;
        [SerializeField] private float groundAheadProbeDistance = 0.65f;
        [SerializeField] private float wallProbeDistance = 0.18f;
        [SerializeField] private float patrolRadius = 4f;
        [SerializeField] private float returnHomeDistance = 6f;

        private Transform target;
        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private SpriteRenderer spriteRenderer;
        private VisualStateAnimator visualState;
        private SpriteSheetAnimator spriteSheetAnimator;
        private DropSpawner dropSpawner;
        private int currentHealth;
        private float nextAttackTime;
        private int facingDirection = -1;
        private Vector2 homePosition;
        private bool dead;

        public bool IsAlive { get { return !dead; } }
        public EnemyDefinition Definition { get { return definition; } }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            visualState = GetComponent<VisualStateAnimator>();
            spriteSheetAnimator = GetComponent<SpriteSheetAnimator>();
            homePosition = transform.position;
        }

        public void Initialize(EnemyDefinition enemyDefinition, Transform playerTarget, DropSpawner spawner)
        {
            definition = enemyDefinition != null ? enemyDefinition : definition;
            target = playerTarget;
            dropSpawner = spawner;
            currentHealth = definition != null ? definition.MaxHealth : 1;
            homePosition = transform.position;
            ApplyDefinitionVisuals();
        }

        private void ApplyDefinitionVisuals()
        {
            if (definition == null || spriteRenderer == null)
            {
                return;
            }

            if (definition.SpriteSheet != null && spriteSheetAnimator != null)
            {
                spriteRenderer.color = Color.white;
                spriteSheetAnimator.Configure(definition.SpriteSheet, 96, 96, 96f);
                if (visualState != null)
                {
                    visualState.SetBaseColor(Color.white);
                }
            }
            else
            {
                spriteRenderer.sprite = SpriteFactory.GetCreatureSilhouetteSprite();
                spriteRenderer.color = definition.PlaceholderColor;
                if (visualState != null)
                {
                    visualState.SetBaseColor(definition.PlaceholderColor);
                }
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

            bool shouldReturnHome = Mathf.Abs(transform.position.x - homePosition.x) > returnHomeDistance;
            if (shouldReturnHome)
            {
                MoveToward(homePosition.x);
            }
            else if (distance <= definition.ChaseRange && Mathf.Abs(transform.position.x - homePosition.x) <= patrolRadius)
            {
                MoveToward(target.position.x);
            }
            else if (visualState != null)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                visualState.SetMoving(false);
            }
        }

        private void MoveToward(float targetX)
        {
            facingDirection = targetX >= transform.position.x ? 1 : -1;
            float horizontalSpeed = CanMoveInDirection(facingDirection) ? facingDirection * definition.MoveSpeed : 0f;
            body.linearVelocity = new Vector2(horizontalSpeed, body.linearVelocity.y);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDirection;
            transform.localScale = scale;
            if (visualState != null)
            {
                visualState.SetMoving(Mathf.Abs(horizontalSpeed) > 0.01f);
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
            if (definition.AttackLungeForce > 0f)
            {
                body.AddForce(new Vector2(facingDirection * definition.AttackLungeForce, 0.45f), ForceMode2D.Impulse);
            }

            if (visualState != null)
            {
                visualState.TriggerAttack();
            }

            IDamageable damageable = target.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                Vector2 knockback = new Vector2(facingDirection * definition.AttackKnockback, 1.2f);
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

        private bool CanMoveInDirection(int direction)
        {
            // TODO: Validate probe distances per creature size in Unity Play Mode.
            if (bodyCollider == null)
            {
                return true;
            }

            Bounds bounds = bodyCollider.bounds;
            Vector2 footOrigin = new Vector2(
                bounds.center.x + direction * (bounds.extents.x + groundAheadProbeDistance),
                bounds.min.y + 0.08f);
            if (!HasBlockingHit(Physics2D.RaycastAll(footOrigin, Vector2.down, 0.35f, platformMask)))
            {
                return false;
            }

            Vector2 wallOrigin = new Vector2(bounds.center.x, bounds.center.y);
            return !HasBlockingHit(Physics2D.RaycastAll(wallOrigin, Vector2.right * direction, bounds.extents.x + wallProbeDistance, platformMask));
        }

        private bool HasBlockingHit(RaycastHit2D[] hits)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hitCollider = hits[i].collider;
                if (hitCollider != null && hitCollider != bodyCollider && !hitCollider.isTrigger)
                {
                    return true;
                }
            }

            return false;
        }

        private void SpawnDrops()
        {
            if (dropSpawner == null || definition == null)
            {
                return;
            }

            for (int i = 0; i < definition.Drops.Count; i++)
            {
                TrySpawnDrop(definition.Drops[i]);
            }

            if (definition.DropTable == null)
            {
                return;
            }

            for (int i = 0; i < definition.DropTable.RareEntries.Count; i++)
            {
                TrySpawnDrop(definition.DropTable.RareEntries[i]);
            }
        }

        private void TrySpawnDrop(DropTableEntry drop)
        {
            if (drop == null || drop.Item == null || UnityEngine.Random.value > drop.Chance)
            {
                return;
            }

            int quantity = UnityEngine.Random.Range(drop.MinQuantity, drop.MaxQuantity + 1);
            dropSpawner.SpawnDrop(drop.Item, quantity, transform.position + Vector3.up * 0.35f);
        }

        private IEnumerator DestroyAfterDeath()
        {
            yield return new WaitForSeconds(0.45f);
            Destroy(gameObject);
        }
    }
}
