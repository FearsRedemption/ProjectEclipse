using ProjectEclipse.Items;

namespace ProjectEclipse.UI
{
    public class ItemHoverState
    {
        public ItemDefinition Item { get; private set; }
        public int Quantity { get; private set; }

        public void Set(ItemDefinition item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }
    }
}
