using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Enemies
{
    [System.Serializable]
    public class DropTableEntry
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int minQuantity = 1;
        [SerializeField] private int maxQuantity = 1;
        [SerializeField] [Range(0f, 1f)] private float chance = 1f;

        public ItemDefinition Item { get { return item; } }
        public int MinQuantity { get { return Mathf.Max(1, minQuantity); } }
        public int MaxQuantity { get { return Mathf.Max(MinQuantity, maxQuantity); } }
        public float Chance { get { return Mathf.Clamp01(chance); } }

        public DropTableEntry(ItemDefinition dropItem, int min, int max, float dropChance)
        {
            item = dropItem;
            minQuantity = Mathf.Max(1, min);
            maxQuantity = Mathf.Max(minQuantity, max);
            chance = Mathf.Clamp01(dropChance);
        }
    }
}

