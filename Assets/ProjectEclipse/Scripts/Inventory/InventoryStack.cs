using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Inventory
{
    [System.Serializable]
    public class InventoryStack
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int quantity;

        public ItemDefinition Item { get { return item; } }
        public int Quantity { get { return quantity; } }
        public int SpaceRemaining { get { return item == null ? 0 : Mathf.Max(0, item.StackLimit - quantity); } }

        public InventoryStack(ItemDefinition stackItem, int amount)
        {
            item = stackItem;
            quantity = Mathf.Clamp(amount, 0, stackItem != null ? stackItem.StackLimit : 999);
        }

        public bool CanStackWith(ItemDefinition other)
        {
            return item == other && SpaceRemaining > 0;
        }

        public int Add(int amount)
        {
            if (item == null || amount <= 0)
            {
                return amount;
            }

            int accepted = Mathf.Min(amount, SpaceRemaining);
            quantity += accepted;
            return amount - accepted;
        }

        public int Remove(int amount)
        {
            int removed = Mathf.Clamp(amount, 0, quantity);
            quantity -= removed;
            return removed;
        }

        public InventoryStack Copy()
        {
            return new InventoryStack(item, quantity);
        }
    }
}

