using UnityEngine;

namespace ProjectEclipse.Progression
{
    [CreateAssetMenu(menuName = "Project Eclipse/Progression/Boss Definition")]
    public class BossDefinition : ScriptableObject
    {
        [SerializeField] private string bossId = "boss";
        [SerializeField] private string displayName = "Boss";
        [SerializeField] private ResourceTier resourceTier = ResourceTier.Wood;
        [SerializeField] private CraftingTier expectedCraftingTier = CraftingTier.Camp;
        [SerializeField] private int recommendedLevel = 1;
        [SerializeField] private bool unlocksNextStage = true;

        public string BossId { get { return bossId; } }
        public string DisplayName { get { return displayName; } }
        public ResourceTier ResourceTier { get { return resourceTier; } }
        public CraftingTier ExpectedCraftingTier { get { return expectedCraftingTier; } }
        public int RecommendedLevel { get { return Mathf.Max(1, recommendedLevel); } }
        public bool UnlocksNextStage { get { return unlocksNextStage; } }
    }
}
