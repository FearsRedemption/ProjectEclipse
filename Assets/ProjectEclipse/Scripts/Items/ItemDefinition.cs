using UnityEngine;

namespace ProjectEclipse.Items
{
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemId = "item";
        [SerializeField] private string displayName = "Item";
        [SerializeField] private ItemCategory category = ItemCategory.Material;
        [SerializeField] private int stackLimit = 999;
        [SerializeField] private Color placeholderColor = Color.white;
        [SerializeField] private Sprite icon;

        public string ItemId { get { return itemId; } }
        public string DisplayName { get { return displayName; } }
        public ItemCategory Category { get { return category; } }
        public int StackLimit { get { return Mathf.Max(1, stackLimit); } }
        public Color PlaceholderColor { get { return placeholderColor; } }
        public Sprite Icon { get { return icon; } }

        public void Configure(
            string id,
            string name,
            ItemCategory itemCategory,
            Color debugColor,
            int maxStack = 999,
            Sprite itemIcon = null)
        {
            itemId = id;
            displayName = name;
            category = itemCategory;
            placeholderColor = debugColor;
            stackLimit = Mathf.Clamp(maxStack, 1, 999);
            icon = itemIcon;
        }
    }
}

