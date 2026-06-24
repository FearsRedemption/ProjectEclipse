using System.Collections.Generic;
using ProjectEclipse.Combat;
using UnityEngine;

namespace ProjectEclipse.Player
{
    [CreateAssetMenu(menuName = "Project Eclipse/Player/Class Branch Definition")]
    public class ClassBranchDefinition : ScriptableObject
    {
        [SerializeField] private string branchId = "class-branch";
        [SerializeField] private string displayName = "Class Branch";
        [SerializeField] private PlayerClassArchetype archetype = PlayerClassArchetype.Warrior;
        [SerializeField] private ClassStageDefinition requiredStage;
        [SerializeField] private string unlockFlag;
        [SerializeField] [TextArea] private string branchRole;
        [SerializeField] private List<SkillDefinition> grantedSkills = new List<SkillDefinition>();

        public string BranchId { get { return branchId; } }
        public string DisplayName { get { return displayName; } }
        public PlayerClassArchetype Archetype { get { return archetype; } }
        public ClassStageDefinition RequiredStage { get { return requiredStage; } }
        public string UnlockFlag { get { return unlockFlag; } }
        public string BranchRole { get { return branchRole; } }
        public IReadOnlyList<SkillDefinition> GrantedSkills { get { return grantedSkills; } }
    }
}
