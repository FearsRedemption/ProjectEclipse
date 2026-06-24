using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Combat
{
    [CreateAssetMenu(menuName = "Project Eclipse/Combat/Skill Upgrade Definition")]
    public class SkillUpgradeDefinition : ScriptableObject
    {
        [SerializeField] private string upgradeId = "skill-upgrade";
        [SerializeField] private string displayName = "Skill Upgrade";
        [SerializeField] private SkillDefinition skill;
        [SerializeField] private SkillUnlockRequirement unlockRequirement = new SkillUnlockRequirement();
        [SerializeField] private GemCoreType gemCoreRequirement = GemCoreType.None;
        [SerializeField] private ItemDefinition materialRequirement;
        [SerializeField] private int materialCount = 1;
        [SerializeField] private int damageBonus;
        [SerializeField] private float cooldownMultiplier = 1f;
        [SerializeField] private float rangeMultiplier = 1f;
        [SerializeField] [TextArea] private string modifierSummary;

        public string UpgradeId { get { return upgradeId; } }
        public string DisplayName { get { return displayName; } }
        public SkillDefinition Skill { get { return skill; } }
        public SkillUnlockRequirement UnlockRequirement { get { return unlockRequirement; } }
        public GemCoreType GemCoreRequirement { get { return gemCoreRequirement; } }
        public ItemDefinition MaterialRequirement { get { return materialRequirement; } }
        public int MaterialCount { get { return Mathf.Max(1, materialCount); } }
        public int DamageBonus { get { return damageBonus; } }
        public float CooldownMultiplier { get { return Mathf.Max(0.01f, cooldownMultiplier); } }
        public float RangeMultiplier { get { return Mathf.Max(0.01f, rangeMultiplier); } }
        public string ModifierSummary { get { return modifierSummary; } }
    }
}
