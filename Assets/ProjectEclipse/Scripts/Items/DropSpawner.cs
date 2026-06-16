using ProjectEclipse.Utilities;
using UnityEngine;

namespace ProjectEclipse.Items
{
    public class DropSpawner : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField] private WorldItemDrop dropPrefab;
#pragma warning restore CS0649

        public WorldItemDrop SpawnDrop(ItemDefinition item, int quantity, Vector3 position)
        {
            WorldItemDrop worldDrop = CreateDropInstance(item, position);
            GameObject drop = worldDrop.gameObject;
            drop.name = item != null ? item.DisplayName + " Drop" : "Drop";
            drop.transform.position = position;

            SpriteRenderer renderer = drop.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = drop.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = item != null && item.WorldDropSprite != null ? item.WorldDropSprite : SpriteFactory.GetSquareSprite(item != null ? item.PlaceholderColor : Color.white);
            renderer.color = Color.white;
            renderer.sortingOrder = 6;

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

            collider.radius = 0.18f;
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
    }
}
