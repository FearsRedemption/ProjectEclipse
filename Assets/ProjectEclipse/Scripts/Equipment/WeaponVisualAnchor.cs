using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Equipment
{
    public class WeaponVisualAnchor : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer weaponRenderer;
        [SerializeField] private Transform weaponAnchor;
        [SerializeField] private float maxVisualScale = 0.2f;

        private int facingDirection = 1;

        private void Awake()
        {
            if (weaponAnchor == null)
            {
                weaponAnchor = transform;
            }

            if (weaponRenderer == null)
            {
                weaponRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        public void SetFacingDirection(int direction)
        {
            facingDirection = direction >= 0 ? 1 : -1;
            Vector3 scale = weaponAnchor != null ? weaponAnchor.localScale : transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDirection;

            if (weaponAnchor != null)
            {
                weaponAnchor.localScale = scale;
            }
            else
            {
                transform.localScale = scale;
            }
        }

        public void ApplyWeapon(WeaponDefinition weapon)
        {
            // TODO: Tune weapon anchor offsets against final player hand sprites in Unity.
            if (weaponRenderer == null)
            {
                return;
            }

            weaponRenderer.sprite = weapon != null ? weapon.EquippedVisualSprite : null;
            weaponRenderer.enabled = weaponRenderer.sprite != null;

            if (weaponAnchor == null || weapon == null)
            {
                return;
            }

            weaponAnchor.localPosition = weapon.EquippedVisualOffset;
            weaponAnchor.localRotation = Quaternion.Euler(0f, 0f, weapon.EquippedVisualRotation);
            Vector2 visualScale = ClampVisualScale(weapon.EquippedVisualScale);
            weaponAnchor.localScale = new Vector3(
                Mathf.Abs(visualScale.x) * facingDirection,
                visualScale.y,
                1f);
        }

        private Vector2 ClampVisualScale(Vector2 requestedScale)
        {
            float maxScale = Mathf.Max(0.05f, maxVisualScale);
            float x = Mathf.Clamp(Mathf.Abs(requestedScale.x), 0.05f, maxScale);
            float y = Mathf.Clamp(Mathf.Abs(requestedScale.y), 0.05f, maxScale);
            return new Vector2(x, y);
        }
    }
}
