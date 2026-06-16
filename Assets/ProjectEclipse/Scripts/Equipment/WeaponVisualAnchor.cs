using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Equipment
{
    public class WeaponVisualAnchor : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer weaponRenderer;
        [SerializeField] private Transform weaponAnchor;

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
            weaponAnchor.localScale = new Vector3(
                Mathf.Abs(weapon.EquippedVisualScale.x) * facingDirection,
                weapon.EquippedVisualScale.y,
                1f);
        }
    }
}
