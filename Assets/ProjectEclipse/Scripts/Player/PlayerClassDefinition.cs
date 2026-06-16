using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Player
{
    [CreateAssetMenu(menuName = "Project Eclipse/Player/Class Definition")]
    public class PlayerClassDefinition : ScriptableObject
    {
        [SerializeField] private string classId = "warrior";
        [SerializeField] private string displayName = "Warrior";
        [SerializeField] private PlayerClassArchetype archetype = PlayerClassArchetype.Warrior;
        [SerializeField] private int startingMaxHealth = 24;
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float jumpForce = 13f;
        [SerializeField] private WeaponDefinition startingWeapon;
        [SerializeField] private string classRole = "Close-range weapon fighter";

        public string ClassId { get { return classId; } }
        public string DisplayName { get { return displayName; } }
        public PlayerClassArchetype Archetype { get { return archetype; } }
        public int StartingMaxHealth { get { return Mathf.Max(1, startingMaxHealth); } }
        public float MoveSpeed { get { return Mathf.Max(0.1f, moveSpeed); } }
        public float JumpForce { get { return Mathf.Max(0.1f, jumpForce); } }
        public WeaponDefinition StartingWeapon { get { return startingWeapon; } }
        public string ClassRole { get { return classRole; } }
    }
}
