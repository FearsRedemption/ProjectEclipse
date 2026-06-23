using ProjectEclipse.Items;

namespace ProjectEclipse.Crafting
{
    public class CraftingRequirementLine
    {
        public ItemDefinition Item { get; private set; }
        public string Label { get; private set; }
        public int OwnedQuantity { get; private set; }
        public int RequiredQuantity { get; private set; }
        public int ReservedQuantity { get; private set; }
        public CraftingRequirementStatus Status { get; private set; }
        public string Detail { get; private set; }

        public CraftingRequirementLine(
            ItemDefinition item,
            string label,
            int ownedQuantity,
            int requiredQuantity,
            int reservedQuantity,
            CraftingRequirementStatus status,
            string detail)
        {
            Item = item;
            Label = label;
            OwnedQuantity = ownedQuantity;
            RequiredQuantity = requiredQuantity;
            ReservedQuantity = reservedQuantity;
            Status = status;
            Detail = detail;
        }
    }
}
