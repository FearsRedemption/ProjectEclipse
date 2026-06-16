using ProjectEclipse.Utilities;
using UnityEngine;

namespace ProjectEclipse.Items
{
    public class DropSpawner : MonoBehaviour
    {
        public WorldItemDrop SpawnDrop(ItemDefinition item, int quantity, Vector3 position)
        {
            GameObject drop = new GameObject(item != null ? item.DisplayName + " Drop" : "Drop");
            drop.transform.position = position;
            SpriteRenderer renderer = drop.AddComponent<SpriteRenderer>();
            renderer.sprite = item != null && item.Icon != null ? item.Icon : SpriteFactory.GetSquareSprite(item != null ? item.PlaceholderColor : Color.white);
            renderer.color = Color.white;
            renderer.sortingOrder = 6;

            Rigidbody2D body = drop.AddComponent<Rigidbody2D>();
            body.gravityScale = 2.4f;
            body.AddForce(new Vector2(UnityEngine.Random.Range(-2.8f, 2.8f), UnityEngine.Random.Range(4.4f, 6.4f)), ForceMode2D.Impulse);

            CircleCollider2D collider = drop.AddComponent<CircleCollider2D>();
            collider.radius = 0.18f;
            PhysicsMaterial2D bounceMaterial = new PhysicsMaterial2D("Drop Bounce");
            bounceMaterial.bounciness = 0.35f;
            bounceMaterial.friction = 0.2f;
            collider.sharedMaterial = bounceMaterial;

            WorldItemDrop worldDrop = drop.AddComponent<WorldItemDrop>();
            worldDrop.Initialize(item, quantity);
            return worldDrop;
        }
    }
}
