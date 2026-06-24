using ProjectEclipse.Player;
using UnityEngine;

namespace ProjectEclipse.Combat
{
    [CreateAssetMenu(menuName = "Project Eclipse/Combat/Skill Definition")]
    public class SkillDefinition : ScriptableObject
    {
        [SerializeField] private string skillId = "skill";
        [SerializeField] private string displayName = "Skill";
        [SerializeField] private PlayerClassArchetype archetype = PlayerClassArchetype.Warrior;
        [SerializeField] private SkillSlot slot = SkillSlot.SkillQ;
        [SerializeField] private CombatAction action = CombatAction.SkillQ;
        [SerializeField] private SkillUnlockRequirement unlockRequirement = new SkillUnlockRequirement();
        [SerializeField] [TextArea] private string behaviorDirection;
        [SerializeField] [TextArea] private string progressionNotes;

        public string SkillId { get { return skillId; } }
        public string DisplayName { get { return displayName; } }
        public PlayerClassArchetype Archetype { get { return archetype; } }
        public SkillSlot Slot { get { return slot; } }
        public CombatAction Action { get { return action; } }
        public SkillUnlockRequirement UnlockRequirement { get { return unlockRequirement; } }
        public string BehaviorDirection { get { return behaviorDirection; } }
        public string ProgressionNotes { get { return progressionNotes; } }
    }
}
