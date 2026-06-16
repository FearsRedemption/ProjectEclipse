using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Combat
{
    [CreateAssetMenu(menuName = "Project Eclipse/Combat/Combat Action Definition")]
    public class CombatActionDefinition : ScriptableObject
    {
        [SerializeField] private string actionId = "action";
        [SerializeField] private string displayName = "Action";
        [SerializeField] private CombatAction action = CombatAction.MainhandAttack;
        [SerializeField] private WeaponArchetype weaponArchetype = WeaponArchetype.StarterMelee;
        [SerializeField] private string behaviorPlaceholder;

        public string ActionId { get { return actionId; } }
        public string DisplayName { get { return displayName; } }
        public CombatAction Action { get { return action; } }
        public WeaponArchetype WeaponArchetype { get { return weaponArchetype; } }
        public string BehaviorPlaceholder { get { return behaviorPlaceholder; } }
    }
}
