using System.Collections.Generic;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Crafting
{
    [CreateAssetMenu(menuName = "Project Eclipse/Crafting/Crafting Trinket Definition")]
    public class CraftingPortDefinition : ItemDefinition
    {
        [SerializeField] private CraftingPortSlot portSlot = CraftingPortSlot.FurnacePort;
        [SerializeField] private CraftingStationType stationType = CraftingStationType.FurnacePort;
        [SerializeField] private List<CraftingRecipe> allowedRecipes = new List<CraftingRecipe>();
        [SerializeField] private int portLevel = 1;
        [SerializeField] private int laneCount = 1;
        [SerializeField] private float speedMultiplier = 1f;
        [SerializeField] private float fuelEfficiency = 1f;
        [SerializeField] private string fuelRules;
        [SerializeField] private string allowedRecipeCategories;
        [SerializeField] private string specialEffectText;
        [SerializeField] private string upgradeRequirements;

        public CraftingPortSlot PortSlot { get { return portSlot; } }
        public CraftingStationType StationType { get { return stationType; } }
        public IReadOnlyList<CraftingRecipe> AllowedRecipes { get { return allowedRecipes; } }
        public int PortLevel { get { return Mathf.Max(1, portLevel); } }
        public int LaneCount { get { return Mathf.Max(1, laneCount); } }
        public float SpeedMultiplier { get { return Mathf.Max(0.01f, speedMultiplier); } }
        public float FuelEfficiency { get { return Mathf.Max(0.01f, fuelEfficiency); } }
        public string FuelRules { get { return fuelRules; } }
        public string AllowedRecipeCategories { get { return allowedRecipeCategories; } }
        public string SpecialEffectText { get { return specialEffectText; } }
        public string UpgradeRequirements { get { return upgradeRequirements; } }
    }
}
