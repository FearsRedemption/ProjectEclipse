using ProjectEclipse.Combat;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Equipment
{
    public class EquipmentController : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition equippedWeapon;
#pragma warning disable CS0649
        [SerializeField] private ItemDefinition headArmorPlaceholder;
        [SerializeField] private ItemDefinition chestArmorPlaceholder;
        [SerializeField] private ItemDefinition legsArmorPlaceholder;
#pragma warning restore CS0649

        private CombatController combatController;
        private InventoryStore inventory;

        public WeaponDefinition EquippedWeapon { get { return equippedWeapon; } }
        public ItemDefinition HeadArmorPlaceholder { get { return headArmorPlaceholder; } }
        public ItemDefinition ChestArmorPlaceholder { get { return chestArmorPlaceholder; } }
        public ItemDefinition LegsArmorPlaceholder { get { return legsArmorPlaceholder; } }

        private void Awake()
        {
            combatController = GetComponent<CombatController>();
            inventory = GetComponent<InventoryStore>();
        }

        public void Initialize(CombatController combat, InventoryStore store)
        {
            combatController = combat;
            inventory = store;
        }

        public bool TryEquipWeapon(WeaponDefinition weapon)
        {
            if (weapon == null)
            {
                return false;
            }

            equippedWeapon = weapon;
            if (combatController != null)
            {
                combatController.SetWeapon(weapon);
            }

            return true;
        }

        public bool TryEquipFromStorage(ItemDefinition item)
        {
            WeaponDefinition weapon = item as WeaponDefinition;
            if (weapon == null || inventory == null || !inventory.HasItem(weapon, 1))
            {
                return false;
            }

            return TryEquipWeapon(weapon);
        }
    }
}
