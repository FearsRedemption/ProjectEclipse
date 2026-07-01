using System.Collections.Generic;
using ProjectEclipse.Utilities;
using UnityEngine;

namespace ProjectEclipse.Items
{
    public class DropSpawner : MonoBehaviour
    {
        private static readonly HashSet<string> WarnedGeneratedDropArt = new HashSet<string>();

#pragma warning disable CS0649
        [SerializeField] private WorldItemDrop dropPrefab;
#pragma warning restore CS0649
        [SerializeField] private float dropVisualMaxSize = 0.34f;
        [SerializeField] private float dropPhysicsRadius = 0.18f;

        public WorldItemDrop SpawnDrop(ItemDefinition item, int quantity, Vector3 position)
        {
            WorldItemDrop worldDrop = CreateDropInstance(item, position);
            GameObject drop = worldDrop.gameObject;
            drop.name = item != null ? item.DisplayName + " Drop" : "Drop";
            drop.transform.position = position;
            drop.transform.localScale = Vector3.one;

            SpriteRenderer renderer = drop.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = drop.AddComponent<SpriteRenderer>();
            }

            if (item != null && item.WorldDropSprite != null)
            {
                renderer.sprite = item.WorldDropSprite;
            }
            else
            {
                string itemName = item != null ? item.DisplayName : "Unknown Item";
                if (WarnedGeneratedDropArt.Add(itemName))
                {
                    Debug.Log("World drop is using generated fallback art for " + itemName + ". Assign a real WorldDropSprite before final art lock.");
                }

                renderer.sprite = SpriteFactory.GetItemDropSprite(item != null ? item.PlaceholderColor : Color.white);
            }
            renderer.color = Color.white;
            renderer.sortingOrder = 6;
            ApplyVisualScale(drop, renderer);

            Rigidbody2D body = drop.GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = drop.AddComponent<Rigidbody2D>();
            }

            body.gravityScale = 2.4f;
            body.AddForce(new Vector2(UnityEngine.Random.Range(-2.8f, 2.8f), UnityEngine.Random.Range(4.4f, 6.4f)), ForceMode2D.Impulse);

            CircleCollider2D collider = drop.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = drop.AddComponent<CircleCollider2D>();
            }

            collider.radius = GetLocalRadiusForWorldRadius(dropPhysicsRadius, drop.transform);
            PhysicsMaterial2D bounceMaterial = new PhysicsMaterial2D("Drop Bounce");
            bounceMaterial.bounciness = 0.35f;
            bounceMaterial.friction = 0.2f;
            collider.sharedMaterial = bounceMaterial;

            worldDrop.Initialize(item, quantity);
            return worldDrop;
        }

        private WorldItemDrop CreateDropInstance(ItemDefinition item, Vector3 position)
        {
            if (dropPrefab != null)
            {
                return Instantiate(dropPrefab, position, Quaternion.identity);
            }

            GameObject drop = new GameObject(item != null ? item.DisplayName + " Drop" : "Drop");
            return drop.AddComponent<WorldItemDrop>();
        }

        private void ApplyVisualScale(GameObject drop, SpriteRenderer renderer)
        {
            if (drop == null || renderer == null || renderer.sprite == null)
            {
                return;
            }

            Vector2 spriteSize = renderer.sprite.bounds.size;
            float maxDimension = Mathf.Max(spriteSize.x, spriteSize.y);
            if (maxDimension <= 0.0001f)
            {
                return;
            }

            float targetSize = Mathf.Max(0.05f, dropVisualMaxSize);
            float uniformScale = Mathf.Clamp(targetSize / maxDimension, 0.04f, 2f);
            drop.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
        }

        private static float GetLocalRadiusForWorldRadius(float worldRadius, Transform transform)
        {
            if (transform == null)
            {
                return Mathf.Max(0.01f, worldRadius);
            }

            Vector3 scale = transform.lossyScale;
            float largestScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), 0.0001f);
            return Mathf.Max(0.01f, worldRadius) / largestScale;
        }
    }
}
