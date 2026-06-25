using System.Collections.Generic;
using ProjectEclipse.Items;
using ProjectEclipse.Equipment;
using ProjectEclipse.Utilities;
using ProjectEclipse.World;
using UnityEngine;

namespace ProjectEclipse.Combat
{
    public class CombatController : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition equippedWeapon;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField] private LayerMask obstructionMask = ~0;
        [SerializeField] private Vector2 attackOriginOffset = new Vector2(0f, 0.1f);
        [SerializeField] private Color slashColor = new Color(1f, 0.88f, 0.45f, 0.82f);
        [SerializeField] private float slashDuration = 0.14f;
        [SerializeField] private int slashSortingOrder = 12;

        private float nextAttackTime;
        private float nextOffhandTime;
        private VisualStateAnimator visualState;
        private int lastAimFacingDirection = 1;

        public WeaponDefinition EquippedWeapon { get { return equippedWeapon; } }
        public int LastAimFacingDirection { get { return lastAimFacingDirection; } }

        private void Awake()
        {
            visualState = GetComponent<VisualStateAnimator>();
        }

        public void SetWeapon(WeaponDefinition weapon)
        {
            equippedWeapon = weapon;
        }

        public bool TryAttack(int facingDirection)
        {
            if (equippedWeapon == null || Time.time < nextAttackTime)
            {
                return false;
            }

            nextAttackTime = Time.time + equippedWeapon.Cooldown;
            if (visualState != null)
            {
                visualState.TriggerAttack();
            }

            Vector2 aim = GetAimDirection(facingDirection);
            return ExecuteDirectionalHit(aim, equippedWeapon.AttackRange, equippedWeapon.AttackHeight, equippedWeapon.Damage, equippedWeapon.Knockback, 1.6f, false);
        }

        public bool TryOffhandAction(EquipmentDefinition offhand, int facingDirection, bool modified)
        {
            if (offhand == null || Time.time < nextOffhandTime)
            {
                return false;
            }

            if (offhand.EquipmentType == "Runic Ammo")
            {
                return false;
            }

            nextOffhandTime = Time.time + (modified ? 0.65f : 0.45f);
            if (visualState != null)
            {
                visualState.TriggerAttack();
            }

            float shoveRange = modified ? 1.1f : 0.85f;
            int damage = Mathf.Max(1, offhand.Stats.Attack + (modified ? 1 : 0));
            float shoveForce = 2.6f + offhand.Stats.Defense + (modified ? 1.6f : 0f);
            return ExecuteDirectionalHit(GetAimDirection(facingDirection), shoveRange, 0.95f, damage, shoveForce, 1.05f, false);
        }

        public bool CanAttack()
        {
            return equippedWeapon != null && Time.time >= nextAttackTime;
        }

        public Vector2 GetAimDirection(int fallbackFacingDirection)
        {
            Vector2 origin = (Vector2)transform.position + attackOriginOffset;
            Camera camera = Camera.main;
            if (camera == null)
            {
                return FallbackAim(fallbackFacingDirection);
            }

            Vector3 mouse = Input.mousePosition;
            mouse.z = Mathf.Abs(camera.transform.position.z - transform.position.z);
            Vector3 world = camera.ScreenToWorldPoint(mouse);
            Vector2 aim = (Vector2)world - origin;
            if (aim.sqrMagnitude < 0.0001f)
            {
                return FallbackAim(fallbackFacingDirection);
            }

            aim.Normalize();
            lastAimFacingDirection = aim.x >= 0f ? 1 : -1;
            return aim;
        }

        public bool PerformSkillHit(int fallbackFacingDirection, float range, float height, int damage, float knockback, float lift)
        {
            if (visualState != null)
            {
                visualState.TriggerAttack();
            }

            return ExecuteDirectionalHit(GetAimDirection(fallbackFacingDirection), range, height, damage, knockback, lift, false);
        }

        public bool PerformRadialHit(float radius, int damage, float knockback, float lift)
        {
            if (visualState != null)
            {
                visualState.TriggerAttack();
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, targetMask);
            List<IDamageable> damaged = new List<IDamageable>();
            bool hitSomething = false;
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
                if (hit == null || hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive || damaged.Contains(damageable))
                {
                    continue;
                }

                if (HasObstructionBetween(transform.position, hit, damageable))
                {
                    continue;
                }

                Vector2 away = (hit.transform.position - transform.position);
                if (away.sqrMagnitude < 0.0001f)
                {
                    away = Vector2.right * lastAimFacingDirection;
                }
                away.Normalize();
                damageable.TakeDamage(new DamageInfo(damage, gameObject, transform.position, away * knockback + Vector2.up * lift));
                damaged.Add(damageable);
                hitSomething = true;
            }

            return hitSomething;
        }

        private Vector2 GetAttackCenter(float direction)
        {
            float range = equippedWeapon != null ? equippedWeapon.AttackRange : 1f;
            return (Vector2)transform.position + new Vector2(
                attackOriginOffset.x + direction * range * 0.5f,
                attackOriginOffset.y);
        }

        private Vector2 FallbackAim(int fallbackFacingDirection)
        {
            lastAimFacingDirection = fallbackFacingDirection >= 0 ? 1 : -1;
            return Vector2.right * lastAimFacingDirection;
        }

        private bool ExecuteDirectionalHit(Vector2 aim, float range, float height, int damage, float knockback, float lift, bool triggerAnimation)
        {
            if (triggerAnimation && visualState != null)
            {
                visualState.TriggerAttack();
            }

            if (aim.sqrMagnitude < 0.0001f)
            {
                aim = Vector2.right * lastAimFacingDirection;
            }
            aim.Normalize();
            lastAimFacingDirection = aim.x >= 0f ? 1 : -1;

            Vector2 origin = (Vector2)transform.position + attackOriginOffset;
            Vector2 center = origin + aim * Mathf.Max(0.1f, range) * 0.5f;
            Vector2 size = new Vector2(Mathf.Max(0.1f, range), Mathf.Max(0.1f, height));
            float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
            SpawnSlashEffect(center, angle, range, height);
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angle, targetMask);
            List<IDamageable> damaged = new List<IDamageable>();

            bool hitSomething = false;
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
                if (hit == null || hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive || damaged.Contains(damageable))
                {
                    continue;
                }

                if (HasObstructionBetween(origin, hit, damageable))
                {
                    continue;
                }

                Vector2 knockbackVector = aim * Mathf.Max(0f, knockback) + Vector2.up * lift;
                damageable.TakeDamage(new DamageInfo(Mathf.Max(1, damage), gameObject, center, knockbackVector));
                damaged.Add(damageable);
                hitSomething = true;
            }

            return hitSomething;
        }

        private bool HasObstructionBetween(Vector2 origin, Collider2D targetCollider, IDamageable targetDamageable)
        {
            if (targetCollider == null)
            {
                return false;
            }

            Vector2 targetPoint = targetCollider.bounds.center;
            RaycastHit2D[] blockers = Physics2D.LinecastAll(origin, targetPoint, obstructionMask);
            for (int i = 0; i < blockers.Length; i++)
            {
                Collider2D blocker = blockers[i].collider;
                if (blocker == null || blocker.isTrigger || blocker.transform == transform || blocker.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (blocker.GetComponent<OneWayPlatform>() != null)
                {
                    continue;
                }

                if (blocker == targetCollider || blocker.GetComponentInParent<IDamageable>() == targetDamageable)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private void SpawnSlashEffect(Vector2 center, float angle, float range, float height)
        {
            GameObject effect = new GameObject("Attack Slash");
            effect.transform.position = new Vector3(center.x, center.y, transform.position.z - 0.05f);
            effect.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            effect.transform.localScale = new Vector3(Mathf.Max(0.75f, range * 0.9f), Mathf.Max(0.55f, height * 0.72f), 1f);

            SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.GetSlashSprite();
            renderer.color = slashColor;
            renderer.sortingOrder = slashSortingOrder;

            SlashEffect slash = effect.AddComponent<SlashEffect>();
            slash.Initialize(slashColor, slashDuration);
        }

        private void OnDrawGizmosSelected()
        {
            if (equippedWeapon == null)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0.3f, 0.15f, 0.35f);
            Vector2 center = GetAttackCenter(1f);
            Gizmos.DrawCube(center, new Vector3(equippedWeapon.AttackRange, equippedWeapon.AttackHeight, 0.1f));
        }
    }
}
