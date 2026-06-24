using ProjectEclipse.Crafting;
using ProjectEclipse.Items;
using ProjectEclipse.Player;
using ProjectEclipse.Progression;
using UnityEngine;

namespace ProjectEclipse.Combat
{
    [System.Serializable]
    public class SkillUnlockRequirement
    {
        [SerializeField] private SkillUnlockRequirementType requirementType = SkillUnlockRequirementType.None;
        [SerializeField] private BossDefinition requiredMiniBoss;
        [SerializeField] private ItemDefinition requiredItem;
        [SerializeField] private ResourceTier requiredMaterialTier = ResourceTier.Wood;
        [SerializeField] private CraftingTier requiredCraftingTier = CraftingTier.Camp;
        [SerializeField] private CraftingTrinketTier requiredCraftingTrinketTier = CraftingTrinketTier.Crude;
        [SerializeField] private int requiredRouteOrder = 1;
        [SerializeField] private GemCoreType requiredGemCore = GemCoreType.None;
        [SerializeField] private PlayerClassArchetype requiredClass = PlayerClassArchetype.Warrior;
        [SerializeField] private string requiredBranchId;
        [SerializeField] private string requiredFlag;

        public SkillUnlockRequirementType RequirementType { get { return requirementType; } }
        public BossDefinition RequiredMiniBoss { get { return requiredMiniBoss; } }
        public ItemDefinition RequiredItem { get { return requiredItem; } }
        public ResourceTier RequiredMaterialTier { get { return requiredMaterialTier; } }
        public CraftingTier RequiredCraftingTier { get { return requiredCraftingTier; } }
        public CraftingTrinketTier RequiredCraftingTrinketTier { get { return requiredCraftingTrinketTier; } }
        public int RequiredRouteOrder { get { return Mathf.Max(1, requiredRouteOrder); } }
        public GemCoreType RequiredGemCore { get { return requiredGemCore; } }
        public PlayerClassArchetype RequiredClass { get { return requiredClass; } }
        public string RequiredBranchId { get { return requiredBranchId; } }
        public string RequiredFlag { get { return requiredFlag; } }
    }
}
