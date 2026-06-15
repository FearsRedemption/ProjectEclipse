using System.Collections.Generic;
using ProjectEclipse.Enemies;
using UnityEngine;

namespace ProjectEclipse.Progression
{
    public class DimensionTierDefinition : ScriptableObject
    {
        [SerializeField] private string tierId = "tier";
        [SerializeField] private string displayName = "Tier";
        [SerializeField] private string requiredBossDefeatedId;
        [SerializeField] private ResourceTier resourceTier = ResourceTier.Wood;
        [SerializeField] private List<EnemyDefinition> availableEnemies = new List<EnemyDefinition>();
        [SerializeField] private string mainBossPlaceholder = "God Placeholder";
        [SerializeField] private List<string> miniBossPlaceholders = new List<string>();

        public string TierId { get { return tierId; } }
        public string DisplayName { get { return displayName; } }
        public string RequiredBossDefeatedId { get { return requiredBossDefeatedId; } }
        public ResourceTier ResourceTier { get { return resourceTier; } }
        public IReadOnlyList<EnemyDefinition> AvailableEnemies { get { return availableEnemies; } }
        public string MainBossPlaceholder { get { return mainBossPlaceholder; } }
        public IReadOnlyList<string> MiniBossPlaceholders { get { return miniBossPlaceholders; } }

        public void Configure(
            string id,
            string name,
            string requiredBossId,
            ResourceTier tier,
            IEnumerable<EnemyDefinition> enemies,
            string mainBoss,
            IEnumerable<string> miniBosses)
        {
            tierId = id;
            displayName = name;
            requiredBossDefeatedId = requiredBossId;
            resourceTier = tier;
            availableEnemies = new List<EnemyDefinition>(enemies);
            mainBossPlaceholder = mainBoss;
            miniBossPlaceholders = new List<string>(miniBosses);
        }
    }
}

