using UnityEngine;

namespace ProjectEclipse.Items
{
    public class WeaponDefinition : ItemDefinition
    {
        [SerializeField] private WeaponArchetype archetype = WeaponArchetype.StarterMelee;
        [SerializeField] private int damage = 1;
        [SerializeField] private float attackRange = 1.2f;
        [SerializeField] private float attackHeight = 1.1f;
        [SerializeField] private float cooldown = 0.45f;
        [SerializeField] private float knockback = 3f;

        public WeaponArchetype Archetype { get { return archetype; } }
        public int Damage { get { return Mathf.Max(1, damage); } }
        public float AttackRange { get { return Mathf.Max(0.1f, attackRange); } }
        public float AttackHeight { get { return Mathf.Max(0.1f, attackHeight); } }
        public float Cooldown { get { return Mathf.Max(0.05f, cooldown); } }
        public float Knockback { get { return Mathf.Max(0f, knockback); } }

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
            Sprite itemIcon = null)
        {
            Configure(id, name, ItemCategory.Weapon, debugColor, 999, itemIcon);
            archetype = weaponArchetype;
            damage = Mathf.Max(1, weaponDamage);
            attackRange = Mathf.Max(0.1f, range);
            attackHeight = Mathf.Max(0.1f, height);
            cooldown = Mathf.Max(0.05f, attackCooldown);
            knockback = Mathf.Max(0f, weaponKnockback);
        }
    }
}

