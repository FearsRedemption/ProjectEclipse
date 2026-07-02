using UnityEngine;
using ProjectEclipse.Utilities;
using ProjectEclipse.Items;

namespace ProjectEclipse.Equipment
{
    public class EquipmentVisualAnchor : MonoBehaviour
    {
        [SerializeField] private EquipmentSlot slot = EquipmentSlot.Mainhand;
        [SerializeField] private EquippedVisualLayer layer = EquippedVisualLayer.Mainhand;
        [SerializeField] private SpriteRenderer rendererOverride;
        [SerializeField] private Vector2 localOffset;
        [SerializeField] private float localRotation;
        [SerializeField] private Vector2 localScale = Vector2.one;

        private int facingDirection = 1;

        public EquipmentSlot Slot { get { return slot; } }
        public EquippedVisualLayer Layer { get { return layer; } }

        public void Configure(
            EquipmentSlot equipmentSlot,
            EquippedVisualLayer visualLayer,
            SpriteRenderer renderer,
            Vector2 offset,
            float rotation,
            Vector2 scale)
        {
            slot = equipmentSlot;
            layer = visualLayer;
            rendererOverride = renderer;
            localOffset = offset;
            localRotation = rotation;
            localScale = scale == Vector2.zero ? Vector2.one : scale;
            ApplyTransform();
        }

        private void Awake()
        {
            if (rendererOverride == null)
            {
                rendererOverride = GetComponentInChildren<SpriteRenderer>();
            }
        }

        public void SetFacingDirection(int direction)
        {
            facingDirection = direction >= 0 ? 1 : -1;
            ApplyTransform();
        }

        public void Apply(EquipmentDefinition equipment)
        {
            if (rendererOverride == null)
            {
                return;
            }

            WeaponDefinition weapon = equipment as WeaponDefinition;
            if (weapon != null && equipment.Slot == EquipmentSlot.Mainhand)
            {
                ApplyWeapon(weapon);
                return;
            }

            rendererOverride.sprite = GetSprite(equipment);
            rendererOverride.enabled = rendererOverride.sprite != null;
            ApplyTransform();
        }

        private void ApplyWeapon(WeaponDefinition weapon)
        {
            rendererOverride.sprite = weapon.HasExplicitEquippedVisualSprite
                ? weapon.EquippedVisualSprite
                : SpriteFactory.GetWeaponOverlaySprite(weapon);
            rendererOverride.enabled = rendererOverride.sprite != null;

            if (weapon.HasExplicitEquippedVisualSprite)
            {
                localOffset = weapon.EquippedVisualOffset;
                localRotation = weapon.EquippedVisualRotation;
                localScale = ClampVisualScale(weapon.EquippedVisualScale, 0.68f);
            }
            else
            {
                localOffset = Vector2.zero;
                localRotation = 0f;
                localScale = Vector2.one;
            }

            ApplyTransform();
        }

        private static Sprite GetSprite(EquipmentDefinition equipment)
        {
            if (equipment == null)
            {
                return null;
            }

            return equipment.HasExplicitVisualSprite
                ? equipment.VisualSprite
                : SpriteFactory.GetEquipmentOverlaySprite(equipment.Slot, equipment.PlaceholderColor);
        }

        private static Vector2 ClampVisualScale(Vector2 requestedScale, float maxVisualScale)
        {
            float max = Mathf.Max(0.05f, maxVisualScale);
            float x = Mathf.Clamp(Mathf.Abs(requestedScale.x), 0.05f, max);
            float y = Mathf.Clamp(Mathf.Abs(requestedScale.y), 0.05f, max);
            return new Vector2(x, y);
        }

        private void ApplyTransform()
        {
            transform.localPosition = localOffset;
            transform.localRotation = Quaternion.Euler(0f, 0f, localRotation);
            transform.localScale = new Vector3(Mathf.Abs(localScale.x) * facingDirection, localScale.y, 1f);
        }
    }
}
