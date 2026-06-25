using System;
using UnityEngine;

namespace ProjectEclipse.Combat
{
    public class PlayerResource : MonoBehaviour
    {
        [SerializeField] private int maxMp = 100;
        [SerializeField] private float mpRegenPerSecond = 8f;
        [SerializeField] private float regenDelayAfterSpend = 0.45f;

        private int currentMp;
        private float regenAccumulator;
        private float lastSpendTime = -999f;

        public int CurrentMp { get { return currentMp; } }
        public int MaxMp { get { return Mathf.Max(1, maxMp); } }
        public float NormalizedMp { get { return MaxMp > 0 ? currentMp / (float)MaxMp : 0f; } }

        public event Action<int, int> Changed;

        private void Awake()
        {
            currentMp = MaxMp;
        }

        private void Update()
        {
            TickMpRegen();
        }

        public void SetMaxMp(int value, bool refill)
        {
            maxMp = Mathf.Max(1, value);
            currentMp = refill ? MaxMp : Mathf.Clamp(currentMp, 0, MaxMp);
            Changed?.Invoke(currentMp, MaxMp);
        }

        public bool CanSpend(int amount)
        {
            return amount <= 0 || currentMp >= amount;
        }

        public bool TrySpend(int amount)
        {
            int cost = Mathf.Max(0, amount);
            if (!CanSpend(cost))
            {
                return false;
            }

            if (cost == 0)
            {
                return true;
            }

            currentMp = Mathf.Max(0, currentMp - cost);
            regenAccumulator = 0f;
            lastSpendTime = Time.time;
            Changed?.Invoke(currentMp, MaxMp);
            return true;
        }

        public void RestoreMp(int amount)
        {
            int restore = Mathf.Max(0, amount);
            if (restore == 0 || currentMp >= MaxMp)
            {
                return;
            }

            currentMp = Mathf.Min(MaxMp, currentMp + restore);
            Changed?.Invoke(currentMp, MaxMp);
        }

        private void TickMpRegen()
        {
            if (currentMp >= MaxMp || mpRegenPerSecond <= 0f)
            {
                return;
            }

            if (Time.time - lastSpendTime < Mathf.Max(0f, regenDelayAfterSpend))
            {
                return;
            }

            regenAccumulator += mpRegenPerSecond * Time.deltaTime;
            int restore = Mathf.FloorToInt(regenAccumulator);
            if (restore <= 0)
            {
                return;
            }

            regenAccumulator -= restore;
            RestoreMp(restore);
        }
    }
}
