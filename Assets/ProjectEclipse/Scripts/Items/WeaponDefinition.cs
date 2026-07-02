using UnityEngine;

using ProjectEclipse.Equipment;

namespace ProjectEclipse.Items
{
    [CreateAssetMenu(menuName = "Project Eclipse/Items/Weapon Definition")]
    public class WeaponDefinition : EquipmentDefinition
    {
        [SerializeField] private WeaponArchetype archetype = WeaponArchetype.StarterMelee;
        [SerializeField] private int damage = 1;
        [SerializeField] private float attackRange = 1.2f;
        [SerializeField] private float attackHeight = 1.1f;
        [SerializeField] private float cooldown = 0.45f;
        [SerializeField] private float knockback = 3f;
        [SerializeField] private Sprite equippedVisualSprite;
        [SerializeField] private Vector2 equippedVisualOffset = new Vector2(0.28f, -0.02f);
        [SerializeField] private float equippedVisualRotation;
        [SerializeField] private Vector2 equippedVisualScale = Vector2.one;

        public WeaponArchetype Archetype { get { return archetype; } }
        public int Damage { get { return Mathf.Max(1, damage); } }
        public float AttackRange { get { return Mathf.Max(0.1f, attackRange); } }
        public float AttackHeight { get { return Mathf.Max(0.1f, attackHeight); } }
        public float Cooldown { get { return Mathf.Max(0.05f, cooldown); } }
        public float Knockback { get { return Mathf.Max(0f, knockback); } }
        public Sprite EquippedVisualSprite { get { return equippedVisualSprite != null ? equippedVisualSprite : WorldDropSprite; } }
        public bool HasExplicitEquippedVisualSprite { get { return equippedVisualSprite != null; } }
        public Vector2 EquippedVisualOffset { get { return equippedVisualOffset; } }
        public float EquippedVisualRotation { get { return equippedVisualRotation; } }
        public Vector2 EquippedVisualScale { get { return equippedVisualScale; } }

        public void ConfigureWeapon(
            string id,
            string name,
            WeaponArchetype weaponArchetype,
            int weaponDamage,
            float range,
            float height,
            float attackCooldown,
            float weaponKnockback,
            Color debugColor,
            Sprite itemIcon = null,
            Sprite dropSprite = null,
            Sprite inHandSprite = null)
        {
            Configure(id, name, ItemCategory.Weapon, debugColor, 1, itemIcon, dropSprite);
            archetype = weaponArchetype;
            damage = Mathf.Max(1, weaponDamage);
            attackRange = Mathf.Max(0.1f, range);
            attackHeight = Mathf.Max(0.1f, height);
            cooldown = Mathf.Max(0.05f, attackCooldown);
            knockback = Mathf.Max(0f, weaponKnockback);
            equippedVisualSprite = inHandSprite;
        }
    }
}
