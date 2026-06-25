using ProjectEclipse.Equipment;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Combat
{
    public class WarriorSkillController : MonoBehaviour
    {
        [SerializeField] private CombatController combatController;
        [SerializeField] private EquipmentController equipmentController;
        [SerializeField] private PlayerResource resource;
        [SerializeField] private float cleaveCooldown = 1.05f;
        [SerializeField] private float guardBreakCooldown = 1.35f;
        [SerializeField] private float leapStrikeCooldown = 2.2f;
        [SerializeField] private float battleCryCooldown = 4.5f;
        [SerializeField] private float leapImpulse = 5.2f;
        [SerializeField] private int cleaveMpCost = 12;
        [SerializeField] private int guardBreakMpCost = 14;
        [SerializeField] private int leapStrikeMpCost = 20;
        [SerializeField] private int battleCryMpCost = 24;

        private Rigidbody2D body;
        private float nextCleaveTime;
        private float nextGuardBreakTime;
        private float nextLeapStrikeTime;
        private float nextBattleCryTime;

        public string LastFailureReason { get; private set; }

        private void Awake()
        {
            if (combatController == null)
            {
                combatController = GetComponent<CombatController>();
            }
            if (equipmentController == null)
            {
                equipmentController = GetComponent<EquipmentController>();
            }
            if (resource == null)
            {
                resource = GetComponent<PlayerResource>();
            }
            body = GetComponent<Rigidbody2D>();
        }

        public bool TryUseSkill(CombatAction action, int facingDirection, bool modified)
        {
            LastFailureReason = string.Empty;
            switch (action)
            {
                case CombatAction.SkillQ:
                    return TryCleave(facingDirection);
                case CombatAction.SkillE:
                    return TryGuardBreak(facingDirection, modified);
                case CombatAction.SkillR:
                    return TryLeapStrike(facingDirection);
                case CombatAction.SkillF:
                    return TryBattleCry();
                default:
                    return false;
            }
        }

        private bool TryCleave(int facingDirection)
        {
            if (combatController == null || !TryConsumeSkill(ref nextCleaveTime, cleaveCooldown, cleaveMpCost))
            {
                return false;
            }

            int damage = BaseWeaponDamage() + 1;
            combatController.PerformSkillHit(facingDirection, 1.8f, 1.35f, damage, 4.4f, 1.45f);
            return true;
        }

        private bool TryGuardBreak(int facingDirection, bool modified)
        {
            if (combatController == null || !TryConsumeSkill(ref nextGuardBreakTime, guardBreakCooldown, guardBreakMpCost))
            {
                return false;
            }

            EquipmentDefinition offhand = equipmentController != null ? equipmentController.Offhand : null;
            int damage = offhand != null ? Mathf.Max(2, offhand.Stats.Attack + 2) : Mathf.Max(1, BaseWeaponDamage() / 2 + 1);
            float knockback = offhand != null ? 6.2f + offhand.Stats.Defense : 4.2f;
            if (modified)
            {
                knockback += 1.4f;
            }

            combatController.PerformSkillHit(facingDirection, 1.15f, 1.05f, damage, knockback, 1.1f);
            return true;
        }

        private bool TryLeapStrike(int facingDirection)
        {
            if (combatController == null || !TryConsumeSkill(ref nextLeapStrikeTime, leapStrikeCooldown, leapStrikeMpCost))
            {
                return false;
            }

            Vector2 aim = combatController.GetAimDirection(facingDirection);
            if (body != null)
            {
                Vector2 impulse = new Vector2(Mathf.Sign(aim.x == 0f ? facingDirection : aim.x) * leapImpulse, 2.2f);
                body.AddForce(impulse, ForceMode2D.Impulse);
            }

            combatController.PerformSkillHit(facingDirection, 1.45f, 1.25f, BaseWeaponDamage() + 2, 5.2f, 1.5f);
            return true;
        }

        private bool TryBattleCry()
        {
            if (combatController == null || !TryConsumeSkill(ref nextBattleCryTime, battleCryCooldown, battleCryMpCost))
            {
                return false;
            }

            combatController.PerformRadialHit(1.9f, 1, 3.4f, 0.9f);
            return true;
        }

        private int BaseWeaponDamage()
        {
            WeaponDefinition weapon = combatController != null ? combatController.EquippedWeapon : null;
            return weapon != null ? weapon.Damage : 1;
        }

        private bool TryConsumeSkill(ref float nextUseTime, float cooldown, int mpCost)
        {
            if (Time.time < nextUseTime)
            {
                LastFailureReason = "Skill cooling down";
                return false;
            }

            if (resource == null)
            {
                resource = GetComponent<PlayerResource>();
            }

            if (resource != null && !resource.TrySpend(mpCost))
            {
                LastFailureReason = "Not enough MP";
                return false;
            }

            nextUseTime = Time.time + Mathf.Max(0.05f, cooldown);
            return true;
        }
    }
}
