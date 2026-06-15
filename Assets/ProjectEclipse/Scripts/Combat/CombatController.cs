using ProjectEclipse.Items;
using ProjectEclipse.Utilities;
using UnityEngine;

namespace ProjectEclipse.Combat
{
    public class CombatController : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition equippedWeapon;
        [SerializeField] private LayerMask targetMask = ~0;

        private float nextAttackTime;
        private VisualStateAnimator visualState;

        public WeaponDefinition EquippedWeapon { get { return equippedWeapon; } }

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

            float direction = facingDirection >= 0 ? 1f : -1f;
            Vector2 center = (Vector2)transform.position + new Vector2(direction * equippedWeapon.AttackRange * 0.5f, 0.1f);
            Vector2 size = new Vector2(equippedWeapon.AttackRange, equippedWeapon.AttackHeight);
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, targetMask);

            bool hitSomething = false;
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
                if (hit == null || hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive)
                {
                    continue;
                }

                Vector2 knockback = new Vector2(direction * equippedWeapon.Knockback, 1.6f);
                damageable.TakeDamage(new DamageInfo(equippedWeapon.Damage, gameObject, center, knockback));
                hitSomething = true;
            }

            return hitSomething;
        }

        private void OnDrawGizmosSelected()
        {
            if (equippedWeapon == null)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0.3f, 0.15f, 0.35f);
            Vector2 center = (Vector2)transform.position + new Vector2(equippedWeapon.AttackRange * 0.5f, 0.1f);
            Gizmos.DrawCube(center, new Vector3(equippedWeapon.AttackRange, equippedWeapon.AttackHeight, 0.1f));
        }
    }
}

