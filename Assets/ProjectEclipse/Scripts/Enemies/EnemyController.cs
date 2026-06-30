using System.Collections;
using System.Collections.Generic;
using ProjectEclipse.Progression;
using ProjectEclipse.Combat;
using ProjectEclipse.Equipment;
using ProjectEclipse.Items;
using ProjectEclipse.Utilities;
using ProjectEclipse.World;
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
        [SerializeField] private Vector2 idleWanderSeconds = new Vector2(1.1f, 2.8f);
        [SerializeField] private Vector2 idlePauseSeconds = new Vector2(0.35f, 1.15f);
        [SerializeField] private float bumpCooldown = 1.1f;
        [SerializeField] private float bumpInvulnerabilitySeconds = 1.45f;
        [SerializeField] private float bumpAggroSeconds = 4.5f;
        [SerializeField] private float bumpKnockback = 2.65f;
        [SerializeField] private float earlyDropChanceMultiplier = 1f;
        [SerializeField] private int earlyMaxDropQuantityPerEntry = 2;
        [SerializeField] private int earlyMinDropsPerKill = 1;
        [SerializeField] private int earlyMaxDropsPerKill = 4;
        [SerializeField] private int laterMaxDropsPerKill = 8;
        [SerializeField] private float baseExtraDropChance = 0.35f;
        [SerializeField] private float extraDropChanceFalloff = 0.1f;
        [SerializeField] private float luckExtraDropChancePerPoint = 0.025f;
        [SerializeField] private float rareDropLuckWeightPerPoint = 0.04f;

        private Transform target;
        private Collider2D targetCollider;
        private EquipmentController targetEquipment;
        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private SpriteRenderer spriteRenderer;
        private VisualStateAnimator visualState;
        private SpriteSheetAnimator spriteSheetAnimator;
        private DropSpawner dropSpawner;
        private int currentHealth;
        private float nextAttackTime;
        private float nextRangedCastTime;
        private int facingDirection = -1;
        private Vector2 homePosition;
        private float nextWanderDecisionTime;
        private float idlePauseUntil;
        private int idleWanderDirection = -1;
        private float nextBumpTime;
        private float forcedAggroUntil;
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
            targetCollider = FindTargetCollider(target);
            targetEquipment = target != null ? target.GetComponentInParent<EquipmentController>() : null;
            currentHealth = definition != null ? definition.MaxHealth : 1;
            nextRangedCastTime = Time.time + Random.Range(1.25f, 3.25f);
            dead = false;
            if (bodyCollider != null)
            {
                bodyCollider.enabled = true;
            }
            homePosition = transform.position;
            ApplyDefinitionVisuals();
            IgnoreOtherEnemyCollisions();
            IgnorePlayerCollision();
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
                spriteRenderer.sprite = SpriteFactory.GetCreatureSprite(definition);
                spriteRenderer.color = Color.white;
                if (spriteSheetAnimator != null)
                {
                    spriteSheetAnimator.enabled = false;
                }
                if (visualState != null)
                {
                    visualState.SetBaseColor(Color.white);
                }
            }

            transform.localScale = new Vector3(
                Mathf.Abs(definition.VisualScale.x) * Mathf.Sign(transform.localScale.x == 0f ? 1f : transform.localScale.x),
                Mathf.Abs(definition.VisualScale.y),
                1f);

            BoxCollider2D box = bodyCollider as BoxCollider2D;
            if (box != null)
            {
                box.size = definition.ColliderSize;
            }

            if (visualState != null)
            {
                visualState.CaptureBaseState();
            }
        }

        private void Update()
        {
            if (dead || definition == null || target == null)
            {
                return;
            }

            ApplyBumpContact();
            Vector2 toTarget = (Vector2)(target.position - transform.position);
            float distance = Mathf.Abs(toTarget.x);
            bool canAggroTarget = CanAggroTarget();

            if (canAggroTarget && distance <= definition.AttackRange && Mathf.Abs(toTarget.y) < 1.3f)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                AttackIfReady();
                if (visualState != null)
                {
                    visualState.SetMoving(false);
                }
                return;
            }

            if (canAggroTarget && TryRangedCast(toTarget, distance))
            {
                return;
            }

            bool shouldReturnHome = Mathf.Abs(transform.position.x - homePosition.x) > returnHomeDistance;
            if (shouldReturnHome)
            {
                MoveToward(homePosition.x);
            }
            else if (canAggroTarget && distance <= definition.ChaseRange && Mathf.Abs(transform.position.x - homePosition.x) <= patrolRadius)
            {
                MoveToward(target.position.x);
            }
            else
            {
                WanderAimlessly();
            }
        }

        private bool TryRangedCast(Vector2 toTarget, float horizontalDistance)
        {
            if (definition.RangedCastRange <= 0f
                || horizontalDistance <= definition.AttackRange + 0.25f
                || horizontalDistance > definition.RangedCastRange
                || Mathf.Abs(toTarget.y) > 2.4f
                || Time.time < nextRangedCastTime)
            {
                return false;
            }

            nextRangedCastTime = Time.time + definition.RangedCastCooldown;
            if (Random.value > definition.RangedCastChance || HasBlockingTerrainBetween(GetCastOrigin(), target.position))
            {
                return false;
            }

            StopMoving();
            facingDirection = toTarget.x >= 0f ? 1 : -1;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDirection;
            transform.localScale = scale;
            if (visualState != null)
            {
                visualState.TriggerAttack();
            }

            Vector2 origin = GetCastOrigin();
            Vector2 direction = ((Vector2)target.position + Vector2.up * 0.25f) - origin;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.right * facingDirection;
            }

            GameObject projectileObject = new GameObject(definition.DisplayName + " Cast");
            projectileObject.transform.position = origin;
            projectileObject.AddComponent<SpriteRenderer>();
            projectileObject.AddComponent<Rigidbody2D>();
            CircleCollider2D collider = projectileObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.16f;
            EnemyProjectile2D projectile = projectileObject.AddComponent<EnemyProjectile2D>();
            projectile.Initialize(gameObject, target, direction.normalized, definition.RangedCastDamage, definition.RangedProjectileSpeed, definition.AttackKnockback * 0.75f, definition.RangedProjectileLifetime, definition.PlaceholderColor);
            return true;
        }

        private Vector2 GetCastOrigin()
        {
            if (bodyCollider == null)
            {
                return transform.position + Vector3.up * 0.45f;
            }

            Bounds bounds = bodyCollider.bounds;
            return new Vector2(bounds.center.x + facingDirection * bounds.extents.x * 0.65f, bounds.center.y + bounds.extents.y * 0.18f);
        }

        private bool HasBlockingTerrainBetween(Vector2 origin, Vector2 destination)
        {
            RaycastHit2D[] hits = Physics2D.LinecastAll(origin, destination, platformMask);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hitCollider = hits[i].collider;
                if (hitCollider == null
                    || hitCollider == bodyCollider
                    || hitCollider == targetCollider
                    || hitCollider.isTrigger
                    || hitCollider.GetComponent<OneWayPlatform>() != null
                    || hitCollider.GetComponentInParent<IDamageable>() != null)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private void MoveToward(float targetX)
        {
            facingDirection = targetX >= transform.position.x ? 1 : -1;
            MoveInDirection(facingDirection);
        }

        private void MoveInDirection(int direction)
        {
            facingDirection = direction >= 0 ? 1 : -1;
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

        private void StopMoving()
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            if (visualState != null)
            {
                visualState.SetMoving(false);
            }
        }

        private void WanderAimlessly()
        {
            if (Time.time >= nextWanderDecisionTime)
            {
                bool shouldPause = Random.value < 0.34f;
                if (shouldPause)
                {
                    idlePauseUntil = Time.time + Random.Range(idlePauseSeconds.x, idlePauseSeconds.y);
                }
                else
                {
                    idleWanderDirection = Random.value < 0.5f ? -1 : 1;
                    idlePauseUntil = 0f;
                }

                nextWanderDecisionTime = Time.time + Random.Range(idleWanderSeconds.x, idleWanderSeconds.y);
            }

            if (Time.time < idlePauseUntil)
            {
                StopMoving();
                return;
            }

            MoveInDirection(idleWanderDirection);
            if (Mathf.Abs(body.linearVelocity.x) < 0.01f)
            {
                idleWanderDirection *= -1;
                nextWanderDecisionTime = Time.time + 0.25f;
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

        private void ApplyBumpContact()
        {
            if (Time.time < nextBumpTime || target == null || bodyCollider == null)
            {
                return;
            }

            if (targetCollider == null)
            {
                targetCollider = FindTargetCollider(target);
                IgnorePlayerCollision();
            }

            if (targetCollider == null || !IsTouchingTarget())
            {
                return;
            }

            IDamageable damageable = target.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return;
            }

            int damage = Mathf.Max(1, Mathf.CeilToInt(definition.ContactDamage * 0.5f));
            Vector2 away = (Vector2)(target.position - transform.position);
            if (away.sqrMagnitude < 0.0001f)
            {
                away = Vector2.right * facingDirection;
            }
            away.Normalize();

            Vector2 knockback = away * bumpKnockback + Vector2.up * 1.35f;
            damageable.TakeDamage(new DamageInfo(damage, gameObject, transform.position, knockback, bumpInvulnerabilitySeconds));
            forcedAggroUntil = Time.time + Mathf.Max(0.1f, bumpAggroSeconds);
            nextBumpTime = Time.time + Mathf.Max(0.1f, bumpCooldown);
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

            List<DropTableEntry> commonEntries = new List<DropTableEntry>();
            List<DropTableEntry> rareEntries = new List<DropTableEntry>();
            AddValidDropEntries(definition.Drops, commonEntries);
            if (definition.DropTable != null)
            {
                AddValidDropEntries(definition.DropTable.RareEntries, rareEntries);
            }

            if (commonEntries.Count == 0 && rareEntries.Count == 0)
            {
                return;
            }

            int targetQuantity = RollDropsPerKill();
            int spawnedQuantity = SpawnDropEntry(PickWeightedDrop(commonEntries, rareEntries, false), targetQuantity);
            int safety = 0;
            while (spawnedQuantity < targetQuantity && safety < 16)
            {
                safety++;
                DropTableEntry extraDrop = PickWeightedDrop(commonEntries, rareEntries, true);
                if (extraDrop == null)
                {
                    break;
                }

                spawnedQuantity += SpawnDropEntry(extraDrop, targetQuantity - spawnedQuantity);
            }
        }

        private int SpawnDropEntry(DropTableEntry drop, int remainingQuantity)
        {
            if (drop == null || drop.Item == null || remainingQuantity <= 0)
            {
                return 0;
            }

            int maxQuantity = Mathf.Min(drop.MaxQuantity, GetMaxDropQuantityPerEntry(), remainingQuantity);
            int minQuantity = Mathf.Min(drop.MinQuantity, maxQuantity);
            int quantity = UnityEngine.Random.Range(minQuantity, maxQuantity + 1);
            Vector3 offset = Vector3.up * 0.35f + new Vector3(UnityEngine.Random.Range(-0.16f, 0.16f), UnityEngine.Random.Range(0f, 0.14f), 0f);
            dropSpawner.SpawnDrop(drop.Item, quantity, transform.position + offset);
            return quantity;
        }

        private int RollDropsPerKill()
        {
            int minDrops = GetMinDropsPerKill();
            int maxDrops = Mathf.Max(minDrops, GetMaxDropsPerKill());
            int quantity = minDrops;
            for (int nextDrop = minDrops + 1; nextDrop <= maxDrops; nextDrop++)
            {
                if (UnityEngine.Random.value > GetExtraDropChance(nextDrop - minDrops))
                {
                    break;
                }

                quantity++;
            }

            return quantity;
        }

        private int GetMinDropsPerKill()
        {
            return definition == null || IsEarlyTier(definition.ResourceTier)
                ? Mathf.Max(1, earlyMinDropsPerKill)
                : 1;
        }

        private int GetMaxDropsPerKill()
        {
            return definition == null || IsEarlyTier(definition.ResourceTier)
                ? Mathf.Max(1, earlyMaxDropsPerKill)
                : Mathf.Max(1, laterMaxDropsPerKill);
        }

        private float GetExtraDropChance(int extraDropIndex)
        {
            float falloff = Mathf.Max(0f, extraDropChanceFalloff) * Mathf.Max(0, extraDropIndex - 1);
            float luckBonus = GetPlayerLuck() * Mathf.Max(0f, luckExtraDropChancePerPoint);
            return Mathf.Clamp01(baseExtraDropChance + luckBonus - falloff);
        }

        private DropTableEntry PickWeightedDrop(List<DropTableEntry> commonEntries, List<DropTableEntry> rareEntries, bool includeRareEntries)
        {
            float totalWeight = GetTotalDropWeight(commonEntries, false);
            if (includeRareEntries)
            {
                totalWeight += GetTotalDropWeight(rareEntries, true);
            }

            if (totalWeight <= 0f)
            {
                DropTableEntry fallback = GetFirstValidDrop(commonEntries);
                return fallback != null || !includeRareEntries ? fallback : GetFirstValidDrop(rareEntries);
            }

            float roll = UnityEngine.Random.value * totalWeight;
            DropTableEntry selected = PickWeightedDropFromList(commonEntries, false, ref roll);
            if (selected != null || !includeRareEntries)
            {
                return selected;
            }

            return PickWeightedDropFromList(rareEntries, true, ref roll);
        }

        private DropTableEntry PickWeightedDropFromList(List<DropTableEntry> entries, bool rareEntry, ref float roll)
        {
            if (entries == null)
            {
                return null;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                DropTableEntry entry = entries[i];
                float weight = GetDropWeight(entry, rareEntry);
                if (weight <= 0f)
                {
                    continue;
                }

                roll -= weight;
                if (roll <= 0f)
                {
                    return entry;
                }
            }

            return null;
        }

        private float GetTotalDropWeight(List<DropTableEntry> entries, bool rareEntry)
        {
            if (entries == null)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < entries.Count; i++)
            {
                total += GetDropWeight(entries[i], rareEntry);
            }

            return total;
        }

        private float GetDropWeight(DropTableEntry drop, bool rareEntry)
        {
            if (drop == null || drop.Item == null)
            {
                return 0f;
            }

            float weight = Mathf.Clamp01(drop.Chance * GetDropChanceMultiplier());
            if (rareEntry)
            {
                weight *= 1f + GetPlayerLuck() * Mathf.Max(0f, rareDropLuckWeightPerPoint);
            }

            return Mathf.Max(0f, weight);
        }

        private int GetPlayerLuck()
        {
            return targetEquipment != null ? targetEquipment.TotalLuck : 0;
        }

        private static void AddValidDropEntries(IReadOnlyList<DropTableEntry> source, List<DropTableEntry> target)
        {
            if (source == null || target == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                DropTableEntry entry = source[i];
                if (entry != null && entry.Item != null)
                {
                    target.Add(entry);
                }
            }
        }

        private static DropTableEntry GetFirstValidDrop(List<DropTableEntry> entries)
        {
            if (entries == null)
            {
                return null;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].Item != null)
                {
                    return entries[i];
                }
            }

            return null;
        }

        private float GetDropChanceMultiplier()
        {
            if (definition == null)
            {
                return Mathf.Clamp01(earlyDropChanceMultiplier);
            }

            return IsEarlyTier(definition.ResourceTier) ? Mathf.Clamp01(earlyDropChanceMultiplier) : 0.75f;
        }

        private int GetMaxDropQuantityPerEntry()
        {
            if (definition == null || IsEarlyTier(definition.ResourceTier))
            {
                return Mathf.Max(1, earlyMaxDropQuantityPerEntry);
            }

            return 999;
        }

        private static bool IsEarlyTier(ResourceTier tier)
        {
            return tier == ResourceTier.Wood
                || tier == ResourceTier.Stone
                || tier == ResourceTier.Coal
                || tier == ResourceTier.Copper;
        }

        private void IgnoreOtherEnemyCollisions()
        {
            if (bodyCollider == null)
            {
                return;
            }

            EnemyController[] enemies = FindObjectsByType<EnemyController>();
            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyController other = enemies[i];
                if (other == null || other == this || other.bodyCollider == null)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(bodyCollider, other.bodyCollider, true);
            }
        }

        private void IgnorePlayerCollision()
        {
            if (bodyCollider == null || target == null)
            {
                return;
            }

            Collider2D[] colliders = target.GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D playerCollider = colliders[i];
                if (playerCollider == null || playerCollider.isTrigger)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(bodyCollider, playerCollider, true);
            }
        }

        private bool IsTouchingTarget()
        {
            if (targetCollider == null || bodyCollider == null)
            {
                return false;
            }

            ColliderDistance2D distance = bodyCollider.Distance(targetCollider);
            return distance.isOverlapped || distance.distance <= 0.05f;
        }

        private bool CanAggroTarget()
        {
            if (definition == null || definition.IgnorePlayerAboveGearScore < 0 || Time.time < forcedAggroUntil)
            {
                return true;
            }

            if (targetEquipment == null)
            {
                targetEquipment = target != null ? target.GetComponentInParent<EquipmentController>() : null;
            }

            return targetEquipment == null || targetEquipment.TotalGearScore < definition.IgnorePlayerAboveGearScore;
        }

        private static Collider2D FindTargetCollider(Transform targetTransform)
        {
            if (targetTransform == null)
            {
                return null;
            }

            Collider2D[] colliders = targetTransform.GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && !colliders[i].isTrigger)
                {
                    return colliders[i];
                }
            }

            return null;
        }

        private IEnumerator DestroyAfterDeath()
        {
            yield return new WaitForSeconds(0.45f);
            Destroy(gameObject);
        }
    }
}
