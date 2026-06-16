using System.Collections.Generic;
using UnityEngine;

namespace ProjectEclipse.Progression
{
    [CreateAssetMenu(menuName = "Project Eclipse/Progression/World Tier")]
    public class WorldTierDefinition : ScriptableObject
    {
        [SerializeField] private string worldTierId = "earth";
        [SerializeField] private string displayName = "Earth";
        [SerializeField] private int recommendedLevel = 1;
        [SerializeField] private List<ProgressionStageDefinition> stages = new List<ProgressionStageDefinition>();
        [SerializeField] private BossDefinition gateBoss;

        public string WorldTierId { get { return worldTierId; } }
        public string DisplayName { get { return displayName; } }
        public int RecommendedLevel { get { return Mathf.Max(1, recommendedLevel); } }
        public IReadOnlyList<ProgressionStageDefinition> Stages { get { return stages; } }
        public BossDefinition GateBoss { get { return gateBoss; } }
    }
}
