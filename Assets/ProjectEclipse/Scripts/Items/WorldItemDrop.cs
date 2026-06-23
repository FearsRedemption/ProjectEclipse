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
        [SerializeField] private float collectDelay = 0.35f;
        [SerializeField] private float magnetDelay = 0.45f;
        [SerializeField] private float magnetRadius = 2.8f;
        [SerializeField] private float collectRadius = 0.22f;
        [SerializeField] private float magnetSpeed = 8f;

        private float spawnTime;
        private InventoryStore pickupTarget;
        private Rigidbody2D body;

        public void Initialize(ItemDefinition definition, int amount)
        {
            item = definition;
            quantity = Mathf.Max(1, amount);
            spawnTime = Time.time;
            body = GetComponent<Rigidbody2D>();
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (item == null || Time.time - spawnTime < magnetDelay)
            {
                return;
            }

            if (pickupTarget == null)
            {
                pickupTarget = FindPickupTarget();
            }

            if (pickupTarget == null)
            {
                return;
            }

            Vector2 toTarget = pickupTarget.transform.position - transform.position;
            if (toTarget.magnitude <= collectRadius)
            {
                Collect(pickupTarget);
                return;
            }

            Vector2 velocity = toTarget.normalized * magnetSpeed;
            if (body != null)
            {
                body.linearVelocity = Vector2.Lerp(body.linearVelocity, velocity, Time.deltaTime * 8f);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, pickupTarget.transform.position, magnetSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
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

            pickupTarget = inventory;
            if (Vector2.Distance(transform.position, inventory.transform.position) <= collectRadius)
            {
                Collect(inventory);
            }
        }

        private InventoryStore FindPickupTarget()
        {
            // Future pet collectors can use this same target path: identify the owning
            // InventoryStore, then let the drop magnet toward that collector/player.
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, magnetRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                InventoryStore inventory = hits[i] != null ? hits[i].GetComponentInParent<InventoryStore>() : null;
                if (inventory != null)
                {
                    return inventory;
                }
            }

            return null;
        }

        private void Collect(InventoryStore inventory)
        {
            inventory.AddItem(item, quantity);
            Destroy(gameObject);
        }
    }
}
