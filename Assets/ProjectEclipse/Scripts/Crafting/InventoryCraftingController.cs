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

        public bool TryEquipPort(CraftingPortDefinition port)
        {
            if (port == null || inventory == null || !inventory.HasItem(port, 1))
            {
                return false;
            }

            for (int i = 0; i < equippedPorts.Count; i++)
            {
                if (equippedPorts[i] != null && equippedPorts[i].PortSlot == port.PortSlot)
                {
                    equippedPorts[i] = port;
                    return true;
                }
            }

            equippedPorts.Add(port);
            return true;
        }
    }
}
