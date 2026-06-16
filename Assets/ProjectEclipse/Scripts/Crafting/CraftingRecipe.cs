using System.Collections.Generic;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Crafting
{
    public class CraftingRecipe : ScriptableObject
    {
        [SerializeField] private string recipeId = "recipe";
        [SerializeField] private string displayName = "Recipe";
        [SerializeField] private List<CraftingIngredient> ingredients = new List<CraftingIngredient>();
        [SerializeField] private ItemDefinition outputItem;
        [SerializeField] private int outputQuantity = 1;
        [SerializeField] private bool equipOutputIfWeapon;
        [SerializeField] private CraftingStationType stationType = CraftingStationType.Inventory;

        public string RecipeId { get { return recipeId; } }
        public string DisplayName { get { return displayName; } }
        public IReadOnlyList<CraftingIngredient> Ingredients { get { return ingredients; } }
        public ItemDefinition OutputItem { get { return outputItem; } }
        public int OutputQuantity { get { return Mathf.Max(1, outputQuantity); } }
        public bool EquipOutputIfWeapon { get { return equipOutputIfWeapon; } }
        public CraftingStationType StationType { get { return stationType; } }

        public void Configure(
            string id,
            string name,
            IEnumerable<CraftingIngredient> requiredIngredients,
            ItemDefinition result,
            int resultQuantity,
            bool autoEquipWeapon)
        {
            recipeId = id;
            displayName = name;
            ingredients = new List<CraftingIngredient>(requiredIngredients);
            outputItem = result;
            outputQuantity = Mathf.Max(1, resultQuantity);
            equipOutputIfWeapon = autoEquipWeapon;
        }
    }
}
