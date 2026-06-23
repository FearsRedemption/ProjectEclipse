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
        private InventoryCraftingController inventoryCrafting;

        public IReadOnlyList<CraftingRecipe> Recipes { get { return recipes; } }

        public void Initialize(InventoryStore store, EquipmentController playerEquipment, IEnumerable<CraftingRecipe> availableRecipes)
        {
            inventory = store;
            equipment = playerEquipment;
            recipes = new List<CraftingRecipe>(availableRecipes);
            inventoryCrafting = store != null ? store.GetComponent<InventoryCraftingController>() : null;
            if (inventoryCrafting == null && store != null)
            {
                inventoryCrafting = store.gameObject.AddComponent<InventoryCraftingController>();
            }
            if (inventoryCrafting != null)
            {
                inventoryCrafting.Initialize(store);
            }
        }

        public bool CanCraft(CraftingRecipe recipe)
        {
            if (recipe == null || inventory == null || !inventory.HasIngredients(recipe.Ingredients))
            {
                return false;
            }

            return HasRequiredStation(recipe);
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

        public int CountItem(ItemDefinition item)
        {
            return inventory != null ? inventory.CountItem(item) : 0;
        }

        public bool HasRequiredStation(CraftingRecipe recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            return recipe.StationType == CraftingStationType.Inventory
                || (inventoryCrafting != null && inventoryCrafting.HasPort(recipe.StationType));
        }

        public CraftingPortDefinition GetPortForRecipe(CraftingRecipe recipe)
        {
            if (recipe == null || inventoryCrafting == null || recipe.StationType == CraftingStationType.Inventory)
            {
                return null;
            }

            return inventoryCrafting.GetPort(recipe.StationType);
        }
    }
}
