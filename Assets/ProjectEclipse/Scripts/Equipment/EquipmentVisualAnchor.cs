using UnityEngine;

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

            rendererOverride.sprite = equipment != null ? equipment.VisualSprite : null;
            rendererOverride.enabled = rendererOverride.sprite != null;
            ApplyTransform();
        }

        private void ApplyTransform()
        {
            transform.localPosition = localOffset;
            transform.localRotation = Quaternion.Euler(0f, 0f, localRotation);
            transform.localScale = new Vector3(Mathf.Abs(localScale.x) * facingDirection, localScale.y, 1f);
        }
    }
}
