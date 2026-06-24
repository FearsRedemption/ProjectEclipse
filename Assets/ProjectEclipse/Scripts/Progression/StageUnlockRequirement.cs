using UnityEngine;

namespace ProjectEclipse.Progression
{
    [System.Serializable]
    public class StageUnlockRequirement
    {
        [SerializeField] private int requiredRouteOrder = 1;
        [SerializeField] private ResourceTier requiredResourceTier = ResourceTier.Wood;
        [SerializeField] private CraftingTier requiredCraftingTier = CraftingTier.Camp;
        [SerializeField] private BossDefinition requiredBoss;
        [SerializeField] private bool requiresEnhancedMobsUnlocked;
        [SerializeField] private string requiredFlag;

        public int RequiredRouteOrder { get { return Mathf.Max(1, requiredRouteOrder); } }
        public ResourceTier RequiredResourceTier { get { return requiredResourceTier; } }
        public CraftingTier RequiredCraftingTier { get { return requiredCraftingTier; } }
        public BossDefinition RequiredBoss { get { return requiredBoss; } }
        public bool RequiresEnhancedMobsUnlocked { get { return requiresEnhancedMobsUnlocked; } }
        public string RequiredFlag { get { return requiredFlag; } }
    }
}
