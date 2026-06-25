using UnityEngine;
using ProjectEclipse.Equipment;
using ProjectEclipse.UI;

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
                return Input.GetKey(key);
            }
        }

        [SerializeField] private CombatController combatController;
        [SerializeField] private EquipmentController equipmentController;
        [SerializeField] private WarriorSkillController warriorSkills;
        [SerializeField] private float sprintSpeedMultiplier = 1.35f;
        [SerializeField] private float feedbackSeconds = 1.15f;
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
        public bool HasFeedback { get { return Time.time < feedbackUntil && !string.IsNullOrEmpty(feedbackText); } }
        public string FeedbackText { get { return HasFeedback ? feedbackText : string.Empty; } }

        private string feedbackText = string.Empty;
        private float feedbackUntil;

        private void Awake()
        {
            EnsureReferences();
        }

        private void EnsureReferences()
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
            EnsureReferences();
            int requestedFacingDirection = 0;
            SprintHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (PressedMainhand())
            {
                LastAction = SprintHeld ? CombatAction.ShiftMainhandModifier : CombatAction.MainhandAttack;
                if (combatController != null)
                {
                    bool attacked = combatController.TryAttack(facingDirection);
                    requestedFacingDirection = combatController.LastAimFacingDirection;
                    if (!attacked)
                    {
                        ShowFeedback(combatController.EquippedWeapon == null ? "No mainhand equipped" : "Mainhand not ready");
                    }
                }
                else
                {
                    ShowFeedback("Mainhand unavailable");
                }
            }

            if (PressedOffhand())
            {
                if (!SprintHeld && warriorSkills != null && warriorSkills.TryUseSkill(CombatAction.SkillF, facingDirection, false))
                {
                    LastAction = CombatAction.SkillF;
                    ShowFeedback(GetActionLabel(CombatAction.SkillF));
                    if (combatController != null)
                    {
                        requestedFacingDirection = combatController.LastAimFacingDirection;
                    }
                }
                else if (!SprintHeld && warriorSkills != null && !string.IsNullOrEmpty(warriorSkills.LastFailureReason))
                {
                    LastAction = CombatAction.SkillF;
                    ShowFeedback(warriorSkills.LastFailureReason);
                }
                else if (combatController != null)
                {
                    LastAction = SprintHeld ? CombatAction.ShiftOffhandModifier : CombatAction.OffhandAction;
                    EquipmentDefinition offhand = equipmentController != null ? equipmentController.Offhand : null;
                    bool acted = combatController.TryOffhandAction(offhand, facingDirection, SprintHeld);
                    requestedFacingDirection = combatController.LastAimFacingDirection;
                    if (acted)
                    {
                        ShowFeedback(SprintHeld ? "Heavy offhand shove" : "Offhand shove");
                    }
                    else if (offhand == null)
                    {
                        ShowFeedback("No offhand action equipped");
                    }
                    else if (offhand.EquipmentType == "Runic Ammo")
                    {
                        ShowFeedback("Runic ammo action locked");
                    }
                    else
                    {
                        ShowFeedback("Offhand not ready");
                    }
                }
                else
                {
                    ShowFeedback("Offhand unavailable");
                }
            }

            for (int i = 0; i < actionBindings.Length; i++)
            {
                if (actionBindings[i] != null && actionBindings[i].TryUse())
                {
                    if (warriorSkills != null && warriorSkills.TryUseSkill(actionBindings[i].Action, facingDirection, SprintHeld))
                    {
                        LastAction = actionBindings[i].Action;
                        ShowFeedback(GetActionLabel(actionBindings[i].Action));
                        if (combatController != null)
                        {
                            requestedFacingDirection = combatController.LastAimFacingDirection;
                        }
                    }
                    else
                    {
                        string reason = warriorSkills != null && !string.IsNullOrEmpty(warriorSkills.LastFailureReason)
                            ? warriorSkills.LastFailureReason
                            : GetActionLabel(actionBindings[i].Action) + " not ready";
                        ShowFeedback(reason);
                    }
                }
            }

            return requestedFacingDirection;
        }

        private static bool PressedMainhand()
        {
            return Input.GetKey(KeyCode.J)
                || (!MvpHud.PointerBlocksGameplayInput && Input.GetMouseButton(0))
                || Input.GetKey(KeyCode.LeftControl);
        }

        private static bool PressedOffhand()
        {
            return !MvpHud.PointerBlocksGameplayInput && Input.GetMouseButton(1);
        }

        private void ShowFeedback(string message)
        {
            feedbackText = message;
            feedbackUntil = Time.time + Mathf.Max(0.1f, feedbackSeconds);
        }

        private static string GetActionLabel(CombatAction action)
        {
            switch (action)
            {
                case CombatAction.SkillQ:
                    return "Cleave";
                case CombatAction.SkillE:
                    return "Guard Break";
                case CombatAction.SkillR:
                    return "Leap Strike";
                case CombatAction.SkillF:
                    return "Shout";
                default:
                    return "Skill";
            }
        }
    }
}
