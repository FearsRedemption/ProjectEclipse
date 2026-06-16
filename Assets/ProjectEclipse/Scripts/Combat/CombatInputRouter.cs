using UnityEngine;

namespace ProjectEclipse.Combat
{
    public class CombatInputRouter : MonoBehaviour
    {
        [SerializeField] private CombatController combatController;
        [SerializeField] private float sprintSpeedMultiplier = 1.35f;

        public bool SprintHeld { get; private set; }
        public float SprintSpeedMultiplier { get { return Mathf.Max(1f, sprintSpeedMultiplier); } }
        public CombatAction LastAction { get; private set; }

        private void Awake()
        {
            if (combatController == null)
            {
                combatController = GetComponent<CombatController>();
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
                    combatController.TryOffhandAction(facingDirection, SprintHeld);
                }
            }

            if (Input.GetKeyDown(KeyCode.Q)) { LastAction = CombatAction.SkillQ; }
            if (Input.GetKeyDown(KeyCode.E)) { LastAction = CombatAction.SkillE; }
            if (Input.GetKeyDown(KeyCode.R)) { LastAction = CombatAction.SkillR; }
            if (Input.GetKeyDown(KeyCode.F)) { LastAction = CombatAction.SkillF; }
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
