using System.Collections.Generic;
using ProjectEclipse.Inventory;
using UnityEngine;

namespace ProjectEclipse.Crafting
{
    public class InventoryCraftingController : MonoBehaviour
    {
        [SerializeField] private List<CraftingPortDefinition> equippedPorts = new List<CraftingPortDefinition>();

        private InventoryStore inventory;

        public IReadOnlyList<CraftingPortDefinition> EquippedPorts { get { return equippedPorts; } }

        public void Initialize(InventoryStore store)
        {
            inventory = store;
        }

        public bool HasPort(CraftingStationType stationType)
        {
            if (stationType == CraftingStationType.Inventory)
            {
                return true;
            }

            return GetPort(stationType) != null;
        }

        public CraftingPortDefinition GetPort(CraftingStationType stationType)
        {
            for (int i = 0; i < equippedPorts.Count; i++)
            {
                CraftingPortDefinition port = equippedPorts[i];
                if (port != null && port.StationType == stationType)
                {
                    return port;
                }
            }

            return null;
        }

        public CraftingPortDefinition GetEquippedPort(CraftingPortSlot slot)
        {
            for (int i = 0; i < equippedPorts.Count; i++)
            {
                CraftingPortDefinition port = equippedPorts[i];
                if (port != null && port.PortSlot == slot)
                {
                    return port;
                }
            }

            return null;
        }

        public bool TryEquipPort(CraftingPortDefinition port)
        {
            return TryEquipPortFromStorage(port);
        }

        public bool TryEquipPortFromStorage(CraftingPortDefinition port)
        {
            if (port == null || inventory == null || !inventory.HasItem(port, 1))
            {
                return false;
            }

            CraftingPortDefinition previous = GetEquippedPort(port.PortSlot);
            if (previous == port)
            {
                return false;
            }

            if (!inventory.RemoveItem(port, 1))
            {
                return false;
            }

            for (int i = 0; i < equippedPorts.Count; i++)
            {
                if (equippedPorts[i] != null && equippedPorts[i].PortSlot == port.PortSlot)
                {
                    equippedPorts[i] = port;
                    ReturnPreviousPort(previous);
                    return true;
                }
            }

            equippedPorts.Add(port);
            ReturnPreviousPort(previous);
            return true;
        }

        public bool TryUnequipPort(CraftingPortSlot slot)
        {
            if (inventory == null)
            {
                return false;
            }

            for (int i = 0; i < equippedPorts.Count; i++)
            {
                CraftingPortDefinition port = equippedPorts[i];
                if (port == null || port.PortSlot != slot)
                {
                    continue;
                }

                if (!inventory.AddItem(port, 1))
                {
                    return false;
                }

                equippedPorts.RemoveAt(i);
                return true;
            }

            return false;
        }

        private void ReturnPreviousPort(CraftingPortDefinition previous)
        {
            if (previous != null && inventory != null)
            {
                inventory.AddItem(previous, 1);
            }
        }
    }
}
