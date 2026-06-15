using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Crafting
{
    [System.Serializable]
    public class CraftingIngredient
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int quantity = 1;

        public ItemDefinition Item { get { return item; } }
        public int Quantity { get { return Mathf.Max(1, quantity); } }

        public CraftingIngredient(ItemDefinition ingredientItem, int amount)
        {
            item = ingredientItem;
            quantity = Mathf.Max(1, amount);
        }
    }
}

