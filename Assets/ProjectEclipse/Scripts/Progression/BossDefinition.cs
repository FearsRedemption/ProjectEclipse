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
        [SerializeField] private int routeOrder = 1;
        [SerializeField] private bool unlocksNextStage = true;
        [SerializeField] private bool unlocksEnhancedMobs;
        [SerializeField] [TextArea] private string designDirection;

        public string BossId { get { return bossId; } }
        public string DisplayName { get { return displayName; } }
        public ResourceTier ResourceTier { get { return resourceTier; } }
        public CraftingTier ExpectedCraftingTier { get { return expectedCraftingTier; } }
        public int RouteOrder { get { return Mathf.Max(1, routeOrder); } }
        public bool UnlocksNextStage { get { return unlocksNextStage; } }
        public bool UnlocksEnhancedMobs { get { return unlocksEnhancedMobs; } }
        public string DesignDirection { get { return designDirection; } }
    }
}
