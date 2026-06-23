using UnityEngine;
using ProjectEclipse.Equipment;

namespace ProjectEclipse.Combat
{
    public class CombatInputRouter : MonoBehaviour
    {
        [System.Serializable]
        private class ActionBinding
        {
            [SerializeField] private KeyCode key = KeyCode.Q;
            [SerializeField] private CombatAction action = CombatAction.SkillQ;
            [SerializeField] private float cooldown = 1f;

            public ActionBinding()
            {
            }

            public ActionBinding(KeyCode key, CombatAction action, float cooldown)
            {
                this.key = key;
                this.action = action;
                this.cooldown = cooldown;
            }

            public KeyCode Key { get { return key; } }
            public CombatAction Action { get { return action; } }
            public float Cooldown { get { return Mathf.Max(0.05f, cooldown); } }

            public bool TryUse()
            {
                return Input.GetKeyDown(key);
            }
        }

        [SerializeField] private CombatController combatController;
        [SerializeField] private EquipmentController equipmentController;
        [SerializeField] private WarriorSkillController warriorSkills;
        [SerializeField] private float sprintSpeedMultiplier = 1.35f;
        [SerializeField] private ActionBinding[] actionBindings =
        {
            new ActionBinding(KeyCode.Q, CombatAction.SkillQ, 1f),
            new ActionBinding(KeyCode.E, CombatAction.SkillE, 1f),
            new ActionBinding(KeyCode.R, CombatAction.SkillR, 1.25f),
            new ActionBinding(KeyCode.F, CombatAction.SkillF, 1.25f),
        };

        public bool SprintHeld { get; private set; }
        public float SprintSpeedMultiplier { get { return Mathf.Max(1f, sprintSpeedMultiplier); } }
        public CombatAction LastAction { get; private set; }

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
            if (warriorSkills == null)
            {
                warriorSkills = GetComponent<WarriorSkillController>();
            }
            if (warriorSkills == null)
            {
                warriorSkills = gameObject.AddComponent<WarriorSkillController>();
            }
        }

        public int PollActions(int facingDirection)
        {
            int requestedFacingDirection = 0;
            SprintHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (PressedMainhand())
            {
                LastAction = SprintHeld ? CombatAction.ShiftMainhandModifier : CombatAction.MainhandAttack;
                if (combatController != null)
                {
                    combatController.TryAttack(facingDirection);
                    requestedFacingDirection = combatController.LastAimFacingDirection;
                }
            }

            if (PressedOffhand())
            {
                LastAction = SprintHeld ? CombatAction.ShiftOffhandModifier : CombatAction.OffhandAction;
                if (combatController != null)
                {
                    combatController.TryOffhandAction(equipmentController != null ? equipmentController.Offhand : null, facingDirection, SprintHeld);
                    requestedFacingDirection = combatController.LastAimFacingDirection;
                }
            }

            for (int i = 0; i < actionBindings.Length; i++)
            {
                if (actionBindings[i] != null && actionBindings[i].TryUse())
                {
                    if (warriorSkills != null && warriorSkills.TryUseSkill(actionBindings[i].Action, facingDirection, SprintHeld))
                    {
                        LastAction = actionBindings[i].Action;
                        if (combatController != null)
                        {
                            requestedFacingDirection = combatController.LastAimFacingDirection;
                        }
                    }
                }
            }

            return requestedFacingDirection;
        }

        private static bool PressedMainhand()
        {
            return Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftControl);
        }

        private static bool PressedOffhand()
        {
            return Input.GetMouseButtonDown(1);
        }
    }
}
