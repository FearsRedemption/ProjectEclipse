using UnityEngine;

namespace ProjectEclipse.Equipment
{
    [CreateAssetMenu(menuName = "Project Eclipse/Equipment/Back Item Definition")]
    public class BackItemDefinition : EquipmentDefinition
    {
        [SerializeField] private bool supportsMovementAbility;
        [SerializeField] private string movementAbilityPlaceholder = "Cape, cloak, glider, wings, or flight gear hook.";

        public bool SupportsMovementAbility { get { return supportsMovementAbility; } }
        public string MovementAbilityPlaceholder { get { return movementAbilityPlaceholder; } }
    }
}
