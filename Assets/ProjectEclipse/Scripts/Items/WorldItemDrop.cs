using ProjectEclipse.Inventory;
using UnityEngine;

namespace ProjectEclipse.Items
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class WorldItemDrop : MonoBehaviour
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int quantity = 1;
        [SerializeField] private float collectDelay = 0.25f;

        private float spawnTime;

        public void Initialize(ItemDefinition definition, int amount)
        {
            item = definition;
            quantity = Mathf.Max(1, amount);
            spawnTime = Time.time;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryCollect(collision.collider);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryCollect(other);
        }

        private void TryCollect(Collider2D other)
        {
            if (Time.time - spawnTime < collectDelay || item == null)
            {
                return;
            }

            InventoryStore inventory = other.GetComponentInParent<InventoryStore>();
            if (inventory == null)
            {
                return;
            }

            inventory.AddItem(item, quantity);
            Destroy(gameObject);
        }
    }
}

