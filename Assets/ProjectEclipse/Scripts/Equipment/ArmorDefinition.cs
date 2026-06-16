using UnityEngine;

namespace ProjectEclipse.Equipment
{
    [CreateAssetMenu(menuName = "Project Eclipse/Equipment/Armor Definition")]
    public class ArmorDefinition : EquipmentDefinition
    {
        [SerializeField] private string armorType = "Armor";

        public string ArmorType { get { return armorType; } }
    }
}
