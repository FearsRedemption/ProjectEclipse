using System;
using System.Collections;
using ProjectEclipse.Utilities;
using UnityEngine;

namespace ProjectEclipse.Combat
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private float invulnerabilitySeconds = 0.25f;
        [SerializeField] private float baseRegenPerSecond = 0.35f;
        [SerializeField] private float regenDelayAfterDamage = 3f;

        private int currentHealth;
        private bool invulnerable;
        private bool dead;
        private VisualStateAnimator visualState;
        private float lastDamageTime = -999f;
        private float regenAccumulator;

        public event Action<int, int> Damaged;
        public event Action Died;

        public int MaxHealth { get { return Mathf.Max(1, maxHealth); } }
        public int CurrentHealth { get { return currentHealth; } }
        public bool IsAlive { get { return !dead; } }

        private void Awake()
        {
            currentHealth = MaxHealth;
            visualState = GetComponent<VisualStateAnimator>();
        }

        private void Update()
        {
            TickRegeneration();
        }

        public void SetMaxHealth(int value, bool refill)
        {
            maxHealth = Mathf.Max(1, value);
            if (refill)
            {
                currentHealth = MaxHealth;
            }
            else
            {
                currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
            }
        }

        public void TakeDamage(DamageInfo damage)
        {
            if (dead || invulnerable || damage.Amount <= 0)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - damage.Amount);
            lastDamageTime = Time.time;
            regenAccumulator = 0f;
            Damaged?.Invoke(currentHealth, MaxHealth);

            Rigidbody2D body = GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.AddForce(damage.Knockback, ForceMode2D.Impulse);
            }

            if (visualState != null)
            {
                visualState.TriggerHurt();
            }

            if (currentHealth <= 0)
            {
                dead = true;
                if (visualState != null)
                {
                    visualState.TriggerDie();
                }

                Died?.Invoke();
                return;
            }

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(InvulnerabilityRoutine(GetInvulnerabilitySeconds(damage)));
            }
        }

        private float GetInvulnerabilitySeconds(DamageInfo damage)
        {
            return damage.InvulnerabilitySeconds >= 0f ? damage.InvulnerabilitySeconds : invulnerabilitySeconds;
        }

        private IEnumerator InvulnerabilityRoutine(float seconds)
        {
            invulnerable = true;
            yield return new WaitForSeconds(Mathf.Max(0f, seconds));
            invulnerable = false;
        }

        private void TickRegeneration()
        {
            if (dead || currentHealth >= MaxHealth || baseRegenPerSecond <= 0f)
            {
                return;
            }

            if (Time.time - lastDamageTime < Mathf.Max(0f, regenDelayAfterDamage))
            {
                return;
            }

            regenAccumulator += baseRegenPerSecond * Time.deltaTime;
            int healAmount = Mathf.FloorToInt(regenAccumulator);
            if (healAmount <= 0)
            {
                return;
            }

            regenAccumulator -= healAmount;
            currentHealth = Mathf.Min(MaxHealth, currentHealth + healAmount);
            Damaged?.Invoke(currentHealth, MaxHealth);
        }
    }
}
