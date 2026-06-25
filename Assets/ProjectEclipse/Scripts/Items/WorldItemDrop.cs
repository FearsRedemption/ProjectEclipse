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
        [SerializeField] private float magnetDelay = 0.3f;
        [SerializeField] private float magnetRadius = 4.5f;
        [SerializeField] private float collectRadius = 0.42f;
        [SerializeField] private float magnetSpeed = 11f;

        private float spawnTime;
        private InventoryStore pickupTarget;
        private Rigidbody2D body;
        private Collider2D dropCollider;
        private bool collected;

        public void Initialize(ItemDefinition definition, int amount)
        {
            item = definition;
            quantity = Mathf.Max(1, amount);
            spawnTime = Time.time;
            body = GetComponent<Rigidbody2D>();
            dropCollider = GetComponent<Collider2D>();
        }

        public void DelayPickup(float collectDelaySeconds, float magnetDelaySeconds)
        {
            collectDelay = Mathf.Max(collectDelay, collectDelaySeconds);
            magnetDelay = Mathf.Max(magnetDelay, magnetDelaySeconds);
            pickupTarget = null;
            spawnTime = Time.time;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            dropCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (collected || item == null || Time.time - spawnTime < collectDelay)
            {
                return;
            }

            if (pickupTarget == null || !IsTargetWithinMagnetRange(pickupTarget))
            {
                pickupTarget = FindPickupTarget();
            }

            if (pickupTarget == null)
            {
                return;
            }

            if (CanCollectFrom(pickupTarget))
            {
                Collect(pickupTarget);
                return;
            }

            if (Time.time - spawnTime < magnetDelay)
            {
                return;
            }

            Vector2 targetPosition = GetTargetPosition(pickupTarget);
            Vector2 toTarget = targetPosition - (Vector2)transform.position;
            if (toTarget.sqrMagnitude <= 0.0001f)
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
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, magnetSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryCollectFrom(other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryCollectFrom(collision != null ? collision.collider : null);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryCollectFrom(collision != null ? collision.collider : null);
        }

        private void TryCollectFrom(Collider2D other)
        {
            if (other == null || Time.time - spawnTime < collectDelay || item == null)
            {
                return;
            }

            InventoryStore inventory = other.GetComponentInParent<InventoryStore>();
            if (inventory == null)
            {
                return;
            }

            pickupTarget = inventory;
            Collect(inventory);
        }

        private InventoryStore FindPickupTarget()
        {
            // Future pet collectors can use this same target path: identify the owning
            // InventoryStore, then let the drop magnet toward that collector/player.
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, magnetRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                InventoryStore inventory = hits[i] != null ? hits[i].GetComponentInParent<InventoryStore>() : null;
                if (inventory != null && IsTargetWithinMagnetRange(inventory))
                {
                    return inventory;
                }
            }

            return null;
        }

        private bool IsTargetWithinMagnetRange(InventoryStore inventory)
        {
            if (inventory == null)
            {
                return false;
            }

            Vector2 targetPosition = GetTargetPosition(inventory);
            return Vector2.Distance(transform.position, targetPosition) <= magnetRadius;
        }

        private bool CanCollectFrom(InventoryStore inventory)
        {
            if (inventory == null)
            {
                return false;
            }

            Vector2 targetPosition = GetTargetPosition(inventory);
            return Vector2.Distance(transform.position, targetPosition) <= collectRadius;
        }

        private Vector2 GetTargetPosition(InventoryStore inventory)
        {
            if (inventory == null)
            {
                return transform.position;
            }

            Collider2D[] colliders = inventory.GetComponentsInChildren<Collider2D>();
            Vector2 dropPosition = dropCollider != null ? (Vector2)dropCollider.bounds.center : (Vector2)transform.position;
            float bestDistance = float.MaxValue;
            Vector2 bestPoint = inventory.transform.position;
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D candidate = colliders[i];
                if (candidate == null || !candidate.enabled || candidate.isTrigger)
                {
                    continue;
                }

                Vector2 point = candidate.ClosestPoint(dropPosition);
                float distance = (point - dropPosition).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestPoint = point;
                }
            }

            return bestPoint;
        }

        private void Collect(InventoryStore inventory)
        {
            if (collected || inventory == null)
            {
                return;
            }

            if (!inventory.AddItem(item, quantity))
            {
                return;
            }

            collected = true;
            PickupSparkle.Spawn(transform.position, item != null ? item.PlaceholderColor : Color.white);
            Destroy(gameObject);
        }
    }
}
