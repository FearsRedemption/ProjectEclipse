using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Crafting
{
    public enum CraftingPlanStepState
    {
        Pending,
        Processing,
        Complete,
        Blocked
    }

    public class CraftingPlanStep
    {
        public CraftingRecipe Recipe { get; private set; }
        public int CraftCount { get; private set; }
        public bool IsFinalStep { get; private set; }
        public CraftingPlanStepState State { get; set; }
        public float Progress01 { get; set; }
        public float RemainingSeconds { get; set; }

        public ItemDefinition OutputItem { get { return Recipe != null ? Recipe.OutputItem : null; } }
        public int OutputQuantity { get { return Recipe != null ? Recipe.OutputQuantity * CraftCount : 0; } }
        public CraftingStationType StationType { get { return Recipe != null ? Recipe.StationType : CraftingStationType.Inventory; } }
        public int RequiredPortLevel { get { return Recipe != null ? Recipe.RequiredPortLevel : 1; } }
        public float BaseCraftTimeSeconds { get { return Recipe != null ? Recipe.CraftTimeSeconds * Mathf.Max(1, CraftCount) : 0f; } }

        public CraftingPlanStep(CraftingRecipe recipe, int craftCount, bool isFinalStep)
        {
            Recipe = recipe;
            CraftCount = Mathf.Max(1, craftCount);
            IsFinalStep = isFinalStep;
            State = CraftingPlanStepState.Pending;
        }
    }
}
