using System.Collections.Generic;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Crafting
{
    [CreateAssetMenu(menuName = "Project Eclipse/Crafting/Crafting Port Definition")]
    public class CraftingPortDefinition : ItemDefinition
    {
        [SerializeField] private CraftingPortSlot portSlot = CraftingPortSlot.FurnacePort;
        [SerializeField] private CraftingStationType stationType = CraftingStationType.FurnacePort;
        [SerializeField] private List<CraftingRecipe> allowedRecipes = new List<CraftingRecipe>();
        [SerializeField] private float speedMultiplier = 1f;
        [SerializeField] private string fuelRules;

        public CraftingPortSlot PortSlot { get { return portSlot; } }
        public CraftingStationType StationType { get { return stationType; } }
        public IReadOnlyList<CraftingRecipe> AllowedRecipes { get { return allowedRecipes; } }
        public float SpeedMultiplier { get { return Mathf.Max(0.01f, speedMultiplier); } }
        public string FuelRules { get { return fuelRules; } }
    }
}
