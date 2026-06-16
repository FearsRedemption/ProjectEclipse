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

            private float nextUseTime;

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

            public bool TryUse()
            {
                if (!Input.GetKeyDown(key) || Time.time < nextUseTime)
                {
                    return false;
                }

                nextUseTime = Time.time + Mathf.Max(0.05f, cooldown);
                return true;
            }
        }

        [SerializeField] private CombatController combatController;
        [SerializeField] private EquipmentController equipmentController;
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
        }

        public void PollActions(int facingDirection)
        {
            SprintHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (PressedMainhand())
            {
                LastAction = SprintHeld ? CombatAction.ShiftMainhandModifier : CombatAction.MainhandAttack;
                if (combatController != null)
                {
                    combatController.TryAttack(facingDirection);
                }
            }

            if (PressedOffhand())
            {
                LastAction = SprintHeld ? CombatAction.ShiftOffhandModifier : CombatAction.OffhandAction;
                if (combatController != null)
                {
                    combatController.TryOffhandAction(equipmentController != null ? equipmentController.Offhand : null, facingDirection, SprintHeld);
                }
            }

            for (int i = 0; i < actionBindings.Length; i++)
            {
                if (actionBindings[i] != null && actionBindings[i].TryUse())
                {
                    LastAction = actionBindings[i].Action;
                }
            }
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
