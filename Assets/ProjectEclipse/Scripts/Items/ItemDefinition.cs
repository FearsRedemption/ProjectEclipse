using UnityEngine;
using ProjectEclipse.Progression;

namespace ProjectEclipse.Items
{
    [CreateAssetMenu(menuName = "Project Eclipse/Items/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemId = "item";
        [SerializeField] private string displayName = "Item";
        [SerializeField] private ItemCategory category = ItemCategory.Material;
        [SerializeField] private ResourceTier resourceTier = ResourceTier.Wood;
        [SerializeField] private int stackLimit = 999;
        [SerializeField] private Color placeholderColor = Color.white;
        [SerializeField] private Sprite icon;
        [SerializeField] private Sprite worldDropSprite;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private string droppedBy;
        [SerializeField] private string craftingUsage;

        public string ItemId { get { return itemId; } }
        public string DisplayName { get { return displayName; } }
        public ItemCategory Category { get { return category; } }
        public ResourceTier ResourceTier { get { return resourceTier; } }
        public int StackLimit { get { return Mathf.Max(1, stackLimit); } }
        public Color PlaceholderColor { get { return placeholderColor; } }
        public Sprite Icon { get { return icon; } }
        public Sprite WorldDropSprite { get { return worldDropSprite != null ? worldDropSprite : icon; } }
        public string Description { get { return description; } }
        public string DroppedBy { get { return droppedBy; } }
        public string CraftingUsage { get { return craftingUsage; } }

        public void Configure(
            string id,
            string name,
            ItemCategory itemCategory,
            Color debugColor,
            int maxStack = 999,
            Sprite itemIcon = null,
            Sprite dropSprite = null,
            ResourceTier tier = ResourceTier.Wood)
        {
            itemId = id;
            displayName = name;
            category = itemCategory;
            resourceTier = tier;
            placeholderColor = debugColor;
            stackLimit = Mathf.Clamp(maxStack, 1, 999);
            icon = itemIcon;
            worldDropSprite = dropSprite;
        }
    }
}
