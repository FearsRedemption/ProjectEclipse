using System.Collections.Generic;
using ProjectEclipse.Combat;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Equipment
{
    public class EquipmentController : MonoBehaviour
    {
        [System.Serializable]
        private class EquipmentSlotState
        {
            [SerializeField] private EquipmentSlot slot;
            [SerializeField] private EquipmentDefinition item;

            public EquipmentSlotState()
            {
            }

            public EquipmentSlotState(EquipmentSlot slot, EquipmentDefinition item)
            {
                this.slot = slot;
                this.item = item;
            }

            public EquipmentSlot Slot { get { return slot; } }
            public EquipmentDefinition Item { get { return item; } set { item = value; } }
        }

        [SerializeField] private WeaponDefinition equippedWeapon;
        [SerializeField] private WeaponVisualAnchor weaponVisualAnchor;
        [SerializeField] private CharacterVisualController characterVisuals;
        [SerializeField] private List<EquipmentSlotState> slots = new List<EquipmentSlotState>();
        [SerializeField] private ProjectEclipse.Player.PlayerClassDefinition playerClass;
#pragma warning disable CS0649
        [SerializeField] private ItemDefinition headArmorPlaceholder;
        [SerializeField] private ItemDefinition chestArmorPlaceholder;
        [SerializeField] private ItemDefinition legsArmorPlaceholder;
#pragma warning restore CS0649

        private CombatController combatController;
        private InventoryStore inventory;

        public WeaponDefinition EquippedWeapon { get { return equippedWeapon; } }
        public WeaponDefinition Mainhand { get { return equippedWeapon; } }
        public EquipmentDefinition Offhand { get { return GetEquippedEquipment(EquipmentSlot.Offhand); } }
        public EquipmentDefinition Back { get { return GetEquippedEquipment(EquipmentSlot.Back); } }
        public ItemDefinition HeadArmorPlaceholder { get { return headArmorPlaceholder; } }
        public ItemDefinition ChestArmorPlaceholder { get { return chestArmorPlaceholder; } }
        public ItemDefinition LegsArmorPlaceholder { get { return legsArmorPlaceholder; } }

        private void Awake()
        {
            combatController = GetComponent<CombatController>();
            inventory = GetComponent<InventoryStore>();
            if (weaponVisualAnchor == null)
            {
                weaponVisualAnchor = GetComponentInChildren<WeaponVisualAnchor>();
            }
            if (characterVisuals == null)
            {
                characterVisuals = GetComponentInChildren<CharacterVisualController>();
            }
        }

        public void Initialize(CombatController combat, InventoryStore store)
        {
            combatController = combat;
            inventory = store;
            if (weaponVisualAnchor == null)
            {
                weaponVisualAnchor = GetComponentInChildren<WeaponVisualAnchor>();
            }
            if (characterVisuals == null)
            {
                characterVisuals = GetComponentInChildren<CharacterVisualController>();
            }
            ApplyWeaponVisual();
        }

        public bool TryEquipWeapon(WeaponDefinition weapon)
        {
            equippedWeapon = weapon;
            SetSlot(EquipmentSlot.Mainhand, weapon);
            if (combatController != null)
            {
                combatController.SetWeapon(weapon);
            }

            ApplyWeaponVisual();
            return true;
        }

        public ItemDefinition GetEquippedItem(EquipmentSlot slot)
        {
            EquipmentDefinition equipmentItem = GetEquippedEquipment(slot);
            if (equipmentItem != null)
            {
                return equipmentItem;
            }

            switch (slot)
            {
                case EquipmentSlot.Mainhand:
                    return equippedWeapon;
                case EquipmentSlot.Helmet:
                    return headArmorPlaceholder;
                case EquipmentSlot.Chest:
                    return chestArmorPlaceholder;
                case EquipmentSlot.Boots:
                    return legsArmorPlaceholder;
                default:
                    return null;
            }
        }

        public EquipmentDefinition GetEquippedEquipment(EquipmentSlot slot)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].Slot == slot)
                {
                    return slots[i].Item;
                }
            }

            return null;
        }

        public bool TryEquipEquipment(EquipmentDefinition equipmentItem)
        {
            if (!CanEquip(equipmentItem))
            {
                return false;
            }

            WeaponDefinition weapon = equipmentItem as WeaponDefinition;
            if (weapon != null && equipmentItem.Slot == EquipmentSlot.Mainhand)
            {
                return TryEquipWeapon(weapon);
            }

            SetSlot(equipmentItem.Slot, equipmentItem);
            ApplyEquipmentVisual(equipmentItem.Slot, equipmentItem);
            return true;
        }

        public bool CanEquip(EquipmentDefinition equipmentItem)
        {
            return equipmentItem != null && equipmentItem.CanEquip(playerClass);
        }

        public void SetFacingDirection(int direction)
        {
            if (weaponVisualAnchor != null)
            {
                weaponVisualAnchor.SetFacingDirection(direction);
            }
            if (characterVisuals != null)
            {
                characterVisuals.SetFacingDirection(direction);
            }
        }

        private void ApplyWeaponVisual()
        {
            if (weaponVisualAnchor != null)
            {
                weaponVisualAnchor.ApplyWeapon(equippedWeapon);
            }
            ApplyEquipmentVisual(EquipmentSlot.Mainhand, equippedWeapon);
        }

        public bool TryEquipFromStorage(ItemDefinition item)
        {
            EquipmentDefinition equipmentItem = item as EquipmentDefinition;
            if (equipmentItem == null || inventory == null || !inventory.HasItem(equipmentItem, 1) || !CanEquip(equipmentItem))
            {
                return false;
            }

            EquipmentDefinition previous = equipmentItem.Slot == EquipmentSlot.Mainhand ? equippedWeapon : GetEquippedEquipment(equipmentItem.Slot);
            if (previous == equipmentItem)
            {
                return false;
            }

            if (!inventory.RemoveItem(equipmentItem, 1))
            {
                return false;
            }

            if (!TryEquipEquipment(equipmentItem))
            {
                inventory.AddItem(equipmentItem, 1);
                return false;
            }

            if (previous != null && previous != equipmentItem)
            {
                inventory.AddItem(previous, 1);
            }

            return true;
        }

        public bool TryUnequipToStorage(EquipmentSlot slot)
        {
            if (inventory == null)
            {
                return false;
            }

            EquipmentDefinition equipped = slot == EquipmentSlot.Mainhand ? equippedWeapon : GetEquippedEquipment(slot);
            if (equipped == null)
            {
                return false;
            }

            if (!inventory.AddItem(equipped, 1))
            {
                return false;
            }

            if (slot == EquipmentSlot.Mainhand)
            {
                TryEquipWeapon(null);
            }
            else
            {
                SetSlot(slot, null);
                ApplyEquipmentVisual(slot, null);
            }

            return true;
        }

        private void SetSlot(EquipmentSlot slot, EquipmentDefinition item)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].Slot == slot)
                {
                    slots[i].Item = item;
                    return;
                }
            }

            slots.Add(new EquipmentSlotState(slot, item));
        }

        private void ApplyEquipmentVisual(EquipmentSlot slot, EquipmentDefinition equipmentItem)
        {
            if (characterVisuals != null)
            {
                characterVisuals.ApplyEquipment(slot, equipmentItem);
            }
        }
    }
}
