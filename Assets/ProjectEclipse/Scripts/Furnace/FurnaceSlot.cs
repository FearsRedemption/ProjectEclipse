using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Furnace
{
    [System.Serializable]
    public class FurnaceSlot
    {
        [SerializeField] private InventorySlotCategory category;
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int quantity;

        public InventorySlotCategory Category { get { return category; } }
        public ItemDefinition Item { get { return item; } }
        public int Quantity { get { return quantity; } }
        public bool IsEmpty { get { return item == null || quantity <= 0; } }

        public FurnaceSlot(InventorySlotCategory slotCategory)
        {
            category = slotCategory;
        }

        public bool CanAccept(ItemDefinition candidate)
        {
            if (candidate == null)
            {
                return false;
            }

            if (category == InventorySlotCategory.FurnaceFuel)
            {
                return candidate.ItemId.Contains("coal");
            }

            if (category == InventorySlotCategory.FurnaceInput)
            {
                return candidate.ItemId.Contains("copper") || candidate.Category == ItemCategory.Material;
            }

            return category == InventorySlotCategory.FurnaceOutput;
        }

        public bool Add(ItemDefinition newItem, int amount)
        {
            if (amount <= 0 || !CanAccept(newItem))
            {
                return false;
            }

            if (!IsEmpty && item != newItem)
            {
                return false;
            }

            item = newItem;
            quantity += amount;
            return true;
        }

        public void Clear()
        {
            item = null;
            quantity = 0;
        }
    }
}

