using System.Collections.Generic;
using ProjectEclipse.Enemies;
using UnityEngine;

namespace ProjectEclipse.Progression
{
    [CreateAssetMenu(menuName = "Project Eclipse/Progression/Stage Definition")]
    public class ProgressionStageDefinition : ScriptableObject
    {
        [SerializeField] private string stageId = "stage";
        [SerializeField] private string displayName = "Stage";
        [SerializeField] private WorldTierDefinition worldTier;
        [SerializeField] private ResourceTier resourceTier = ResourceTier.Wood;
        [SerializeField] private CraftingTier craftingTier = CraftingTier.Camp;
        [SerializeField] private int recommendedLevel = 1;
        [SerializeField] private StageUnlockRequirement unlockRequirement = new StageUnlockRequirement();
        [SerializeField] private List<EnemyDefinition> enemies = new List<EnemyDefinition>();
        [SerializeField] private List<EnemyDefinition> enhancedEnemies = new List<EnemyDefinition>();
        [SerializeField] private List<EnemyDefinition> eliteEnemies = new List<EnemyDefinition>();
        [SerializeField] private List<BossDefinition> miniBosses = new List<BossDefinition>();
        [SerializeField] private BossDefinition boss;

        public string StageId { get { return stageId; } }
        public string DisplayName { get { return displayName; } }
        public WorldTierDefinition WorldTier { get { return worldTier; } }
        public ResourceTier ResourceTier { get { return resourceTier; } }
        public CraftingTier CraftingTier { get { return craftingTier; } }
        public int RecommendedLevel { get { return Mathf.Max(1, recommendedLevel); } }
        public StageUnlockRequirement UnlockRequirement { get { return unlockRequirement; } }
        public IReadOnlyList<EnemyDefinition> Enemies { get { return enemies; } }
        public IReadOnlyList<EnemyDefinition> EnhancedEnemies { get { return enhancedEnemies; } }
        public IReadOnlyList<EnemyDefinition> EliteEnemies { get { return eliteEnemies; } }
        public IReadOnlyList<BossDefinition> MiniBosses { get { return miniBosses; } }
        public BossDefinition Boss { get { return boss; } }
    }
}
