using UnityEngine;

namespace ProjectEclipse.Items
{
    [CreateAssetMenu(menuName = "Project Eclipse/Items/Consumable Definition")]
    public class ConsumableDefinition : ItemDefinition
    {
        [SerializeField] private string effectDescription = "Consumable effect placeholder";
        [SerializeField] private float durationSeconds;
        [SerializeField] private float cooldownSeconds = 1f;

        public string EffectDescription { get { return effectDescription; } }
        public float DurationSeconds { get { return Mathf.Max(0f, durationSeconds); } }
        public float CooldownSeconds { get { return Mathf.Max(0f, cooldownSeconds); } }
    }
}
