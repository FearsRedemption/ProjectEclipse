using System.Collections.Generic;
using UnityEngine;

namespace ProjectEclipse.Equipment
{
    public class CharacterVisualController : MonoBehaviour
    {
        [SerializeField] private List<EquipmentVisualAnchor> anchors = new List<EquipmentVisualAnchor>();

        private int facingDirection = 1;

        private void Awake()
        {
            if (anchors.Count == 0)
            {
                GetComponentsInChildren(true, anchors);
            }
        }

        public void SetFacingDirection(int direction)
        {
            facingDirection = direction >= 0 ? 1 : -1;
            for (int i = 0; i < anchors.Count; i++)
            {
                if (anchors[i] != null)
                {
                    anchors[i].SetFacingDirection(facingDirection);
                }
            }
        }

        public void ApplyEquipment(EquipmentSlot slot, EquipmentDefinition equipment)
        {
            for (int i = 0; i < anchors.Count; i++)
            {
                EquipmentVisualAnchor anchor = anchors[i];
                if (anchor != null && anchor.Slot == slot)
                {
                    anchor.Apply(equipment);
                }
            }
        }
    }
}
