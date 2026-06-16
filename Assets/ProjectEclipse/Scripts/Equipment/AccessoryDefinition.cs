using UnityEngine;

namespace ProjectEclipse.Equipment
{
    [CreateAssetMenu(menuName = "Project Eclipse/Equipment/Accessory Definition")]
    public class AccessoryDefinition : EquipmentDefinition
    {
        [SerializeField] private string accessoryType = "Accessory";

        public string AccessoryType { get { return accessoryType; } }
    }
}
