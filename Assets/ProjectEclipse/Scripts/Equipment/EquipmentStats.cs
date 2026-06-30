using UnityEngine;

namespace ProjectEclipse.Equipment
{
    [System.Serializable]
    public class EquipmentStats
    {
        [SerializeField] private int attack;
        [SerializeField] private int defense;
        [SerializeField] private int maxHealth;
        [SerializeField] private int luck;
        [SerializeField] private float moveSpeedBonus;
        [SerializeField] private float jumpForceBonus;
        [SerializeField] private float airControlBonus;
        [SerializeField] private float healthRegenPerSecondBonus;

        public int Attack { get { return Mathf.Max(0, attack); } }
        public int Defense { get { return Mathf.Max(0, defense); } }
        public int MaxHealth { get { return Mathf.Max(0, maxHealth); } }
        public int Luck { get { return Mathf.Max(0, luck); } }
        public float MoveSpeedBonus { get { return moveSpeedBonus; } }
        public float JumpForceBonus { get { return jumpForceBonus; } }
        public float AirControlBonus { get { return airControlBonus; } }
        public float HealthRegenPerSecondBonus { get { return Mathf.Max(0f, healthRegenPerSecondBonus); } }
    }
}
