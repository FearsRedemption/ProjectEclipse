using System.Collections.Generic;
using ProjectEclipse.Progression;
using UnityEngine;

namespace ProjectEclipse.Enemies
{
    public class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string enemyId = "enemy";
        [SerializeField] private string displayName = "Enemy";
        [SerializeField] private ResourceTier resourceTier = ResourceTier.Wood;
        [SerializeField] private int maxHealth = 4;
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float chaseRange = 5f;
        [SerializeField] private float attackRange = 0.8f;
        [SerializeField] private float attackCooldown = 1.2f;
        [SerializeField] private float attackLungeForce = 0.5f;
        [SerializeField] private float attackKnockback = 2.4f;
        [SerializeField] private Vector2 visualScale = Vector2.one;
        [SerializeField] private Vector2 colliderSize = Vector2.one;
        [SerializeField] private Texture2D spriteSheet;
        [SerializeField] private Color placeholderColor = Color.green;
        [SerializeField] private DropTableDefinition dropTable;
        [SerializeField] private List<DropTableEntry> drops = new List<DropTableEntry>();

        public string EnemyId { get { return enemyId; } }
        public string DisplayName { get { return displayName; } }
        public ResourceTier ResourceTier { get { return resourceTier; } }
        public int MaxHealth { get { return Mathf.Max(1, maxHealth); } }
        public int ContactDamage { get { return Mathf.Max(0, contactDamage); } }
        public float MoveSpeed { get { return Mathf.Max(0.1f, moveSpeed); } }
        public float ChaseRange { get { return Mathf.Max(0.1f, chaseRange); } }
        public float AttackRange { get { return Mathf.Max(0.1f, attackRange); } }
        public float AttackCooldown { get { return Mathf.Max(0.1f, attackCooldown); } }
        public float AttackLungeForce { get { return Mathf.Max(0f, attackLungeForce); } }
        public float AttackKnockback { get { return Mathf.Max(0f, attackKnockback); } }
        public Vector2 VisualScale { get { return visualScale; } }
        public Vector2 ColliderSize { get { return colliderSize; } }
        public Texture2D SpriteSheet { get { return spriteSheet; } }
        public Color PlaceholderColor { get { return placeholderColor; } }
        public DropTableDefinition DropTable { get { return dropTable; } }
        public IReadOnlyList<DropTableEntry> Drops { get { return dropTable != null ? dropTable.Entries : drops; } }

        public void Configure(
            string id,
            string name,
            ResourceTier tier,
            int health,
            int damage,
            float speed,
            float detectionRange,
            float meleeRange,
            float cooldown,
            float lungeForce,
            float knockback,
            Vector2 scale,
            Vector2 hitboxSize,
            Texture2D sheet,
            Color debugColor,
            IEnumerable<DropTableEntry> dropEntries,
            DropTableDefinition table = null)
        {
            enemyId = id;
            displayName = name;
            resourceTier = tier;
            maxHealth = Mathf.Max(1, health);
            contactDamage = Mathf.Max(0, damage);
            moveSpeed = Mathf.Max(0.1f, speed);
            chaseRange = Mathf.Max(0.1f, detectionRange);
            attackRange = Mathf.Max(0.1f, meleeRange);
            attackCooldown = Mathf.Max(0.1f, cooldown);
            attackLungeForce = Mathf.Max(0f, lungeForce);
            attackKnockback = Mathf.Max(0f, knockback);
            visualScale = scale;
            colliderSize = hitboxSize;
            spriteSheet = sheet;
            placeholderColor = debugColor;
            dropTable = table;
            drops = new List<DropTableEntry>(dropEntries);
        }
    }
}
