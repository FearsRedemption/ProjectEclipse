using UnityEngine;

namespace ProjectEclipse.Combat
{
    public struct DamageInfo
    {
        public readonly int Amount;
        public readonly GameObject Source;
        public readonly Vector2 Point;
        public readonly Vector2 Knockback;
        public readonly float InvulnerabilitySeconds;

        public DamageInfo(int amount, GameObject source, Vector2 point, Vector2 knockback, float invulnerabilitySeconds = -1f)
        {
            Amount = Mathf.Max(0, amount);
            Source = source;
            Point = point;
            Knockback = knockback;
            InvulnerabilitySeconds = invulnerabilitySeconds;
        }
    }
}
