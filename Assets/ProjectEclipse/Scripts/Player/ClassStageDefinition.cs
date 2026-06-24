using System.Collections.Generic;
using ProjectEclipse.Progression;
using UnityEngine;

namespace ProjectEclipse.Player
{
    [CreateAssetMenu(menuName = "Project Eclipse/Player/Class Stage Definition")]
    public class ClassStageDefinition : ScriptableObject
    {
        [SerializeField] private string stageId = "class-stage";
        [SerializeField] private string displayName = "Class Stage";
        [SerializeField] private PlayerClassArchetype archetype = PlayerClassArchetype.Warrior;
        [SerializeField] private ResourceTier materialTierRequirement = ResourceTier.Wood;
        [SerializeField] private int routeOrder = 1;
        [SerializeField] private string routeRequirement;
        [SerializeField] private string trialRequirement;
        [SerializeField] private List<ClassBranchDefinition> availableBranches = new List<ClassBranchDefinition>();

        public string StageId { get { return stageId; } }
        public string DisplayName { get { return displayName; } }
        public PlayerClassArchetype Archetype { get { return archetype; } }
        public ResourceTier MaterialTierRequirement { get { return materialTierRequirement; } }
        public int RouteOrder { get { return Mathf.Max(1, routeOrder); } }
        public string RouteRequirement { get { return routeRequirement; } }
        public string TrialRequirement { get { return trialRequirement; } }
        public IReadOnlyList<ClassBranchDefinition> AvailableBranches { get { return availableBranches; } }
    }
}
