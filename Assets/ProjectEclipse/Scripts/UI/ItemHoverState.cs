using ProjectEclipse.Crafting;
using ProjectEclipse.Equipment;
using ProjectEclipse.Items;

namespace ProjectEclipse.UI
{
    public enum ItemHoverKind
    {
        None,
        Item,
        EquipmentSlot,
        CraftingPortSlot
    }

    public class ItemHoverState
    {
        public ItemDefinition Item { get; private set; }
        public int Quantity { get; private set; }
        public ItemHoverKind Kind { get; private set; }
        public EquipmentSlot EquipmentSlot { get; private set; }
        public CraftingPortSlot CraftingPortSlot { get; private set; }
        public string SlotLabel { get; private set; }
        public string SlotDescription { get; private set; }

        public bool HasHover { get { return Kind != ItemHoverKind.None; } }

        public void Set(ItemDefinition item, int quantity)
        {
            SetItem(item, quantity);
        }

        public void SetItem(ItemDefinition item, int quantity)
        {
            Item = item;
            Quantity = quantity;
            Kind = item != null ? ItemHoverKind.Item : ItemHoverKind.None;
            SlotLabel = string.Empty;
            SlotDescription = string.Empty;
        }

        public void SetEquipmentSlot(EquipmentSlot slot, string label, string description)
        {
            Item = null;
            Quantity = 0;
            Kind = ItemHoverKind.EquipmentSlot;
            EquipmentSlot = slot;
            SlotLabel = label;
            SlotDescription = description;
        }

        public void SetCraftingPortSlot(CraftingPortSlot slot, string label, string description)
        {
            Item = null;
            Quantity = 0;
            Kind = ItemHoverKind.CraftingPortSlot;
            CraftingPortSlot = slot;
            SlotLabel = label;
            SlotDescription = description;
        }
    }
}
