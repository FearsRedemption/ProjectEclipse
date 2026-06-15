using System.Collections.Generic;
using ProjectEclipse.Equipment;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Crafting
{
    public class CraftingSystem : MonoBehaviour
    {
        [SerializeField] private List<CraftingRecipe> recipes = new List<CraftingRecipe>();

        private InventoryStore inventory;
        private EquipmentController equipment;

        public IReadOnlyList<CraftingRecipe> Recipes { get { return recipes; } }

        public void Initialize(InventoryStore store, EquipmentController playerEquipment, IEnumerable<CraftingRecipe> availableRecipes)
        {
            inventory = store;
            equipment = playerEquipment;
            recipes = new List<CraftingRecipe>(availableRecipes);
        }

        public bool CanCraft(CraftingRecipe recipe)
        {
            return recipe != null && inventory != null && inventory.HasIngredients(recipe.Ingredients);
        }

        public bool TryCraft(CraftingRecipe recipe)
        {
            if (!CanCraft(recipe) || !inventory.ConsumeIngredients(recipe.Ingredients))
            {
                return false;
            }

            inventory.AddItem(recipe.OutputItem, recipe.OutputQuantity);

            if (recipe.EquipOutputIfWeapon && equipment != null)
            {
                WeaponDefinition weapon = recipe.OutputItem as WeaponDefinition;
                if (weapon != null)
                {
                    equipment.TryEquipWeapon(weapon);
                }
            }

            return true;
        }
    }
}

