using System.Collections.Generic;
using ProjectEclipse.Utilities;
using UnityEngine;

namespace ProjectEclipse.Equipment
{
    public class CharacterVisualController : MonoBehaviour
    {
        [SerializeField] private List<EquipmentVisualAnchor> anchors = new List<EquipmentVisualAnchor>();
        [SerializeField] private bool buildRuntimePaperDoll = true;
        [SerializeField] private int baseSortingOrder = 10;

        private int facingDirection = 1;
        private SpriteRenderer baseRenderer;

        private void Awake()
        {
            EnsureRuntimeLayers();
        }

        public void EnsureRuntimeLayers()
        {
            if (baseRenderer == null)
            {
                baseRenderer = GetComponent<SpriteRenderer>();
            }

            if (anchors.Count == 0)
            {
                GetComponentsInChildren(true, anchors);
            }

            if (!buildRuntimePaperDoll)
            {
                return;
            }

            SpriteSheetAnimator sheetAnimator = GetComponent<SpriteSheetAnimator>();
            if (sheetAnimator != null)
            {
                sheetAnimator.enabled = false;
            }

            if (baseRenderer != null)
            {
                baseRenderer.sprite = SpriteFactory.GetPlayerBaseBodySprite();
                baseRenderer.color = Color.white;
                baseRenderer.sortingOrder = baseSortingOrder;
                baseRenderer.enabled = true;
            }

            CreateStaticLayer("PaperDoll Undershirt", SpriteFactory.GetPlayerUndershirtSprite(), baseSortingOrder + 1);
            CreateStaticLayer("PaperDoll Shorts", SpriteFactory.GetPlayerShortsSprite(), baseSortingOrder + 2);
            CreateStaticLayer("PaperDoll Hair Face", SpriteFactory.GetPlayerHairFaceSprite(), baseSortingOrder + 7);
            EnsureAnchor(EquipmentSlot.Back, EquippedVisualLayer.Back, baseSortingOrder - 1);
            EnsureAnchor(EquipmentSlot.Boots, EquippedVisualLayer.Boots, baseSortingOrder + 3);
            EnsureAnchor(EquipmentSlot.Chest, EquippedVisualLayer.Chest, baseSortingOrder + 4);
            EnsureAnchor(EquipmentSlot.Gloves, EquippedVisualLayer.Gloves, baseSortingOrder + 5);
            EnsureAnchor(EquipmentSlot.Helmet, EquippedVisualLayer.Helmet, baseSortingOrder + 8);
            EnsureAnchor(EquipmentSlot.Offhand, EquippedVisualLayer.Offhand, baseSortingOrder + 9);
            EnsureAnchor(EquipmentSlot.Mainhand, EquippedVisualLayer.Mainhand, baseSortingOrder + 10);
            EnsureAnchor(EquipmentSlot.Necklace, EquippedVisualLayer.Accessory, baseSortingOrder + 11);
            EnsureAnchor(EquipmentSlot.Belt, EquippedVisualLayer.Accessory, baseSortingOrder + 11);
            EnsureAnchor(EquipmentSlot.Ring1, EquippedVisualLayer.Accessory, baseSortingOrder + 11);
            EnsureAnchor(EquipmentSlot.Ring2, EquippedVisualLayer.Accessory, baseSortingOrder + 11);
            EnsureAnchor(EquipmentSlot.Earring1, EquippedVisualLayer.Accessory, baseSortingOrder + 11);
            EnsureAnchor(EquipmentSlot.Earring2, EquippedVisualLayer.Accessory, baseSortingOrder + 11);
            SetFacingDirection(facingDirection);
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
            EnsureRuntimeLayers();
            for (int i = 0; i < anchors.Count; i++)
            {
                EquipmentVisualAnchor anchor = anchors[i];
                if (anchor != null && anchor.Slot == slot)
                {
                    anchor.Apply(equipment);
                }
            }
        }

        private void CreateStaticLayer(string layerName, Sprite sprite, int sortingOrder)
        {
            SpriteRenderer renderer = GetOrCreateLayerRenderer(layerName, sortingOrder);
            renderer.sprite = sprite;
            renderer.enabled = sprite != null;
        }

        private void EnsureAnchor(EquipmentSlot slot, EquippedVisualLayer layer, int sortingOrder)
        {
            for (int i = 0; i < anchors.Count; i++)
            {
                if (anchors[i] != null && anchors[i].Slot == slot)
                {
                    return;
                }
            }

            string layerName = "PaperDoll " + slot;
            SpriteRenderer renderer = GetOrCreateLayerRenderer(layerName, sortingOrder);
            EquipmentVisualAnchor anchor = renderer.GetComponent<EquipmentVisualAnchor>();
            if (anchor == null)
            {
                anchor = renderer.gameObject.AddComponent<EquipmentVisualAnchor>();
            }

            anchor.Configure(slot, layer, renderer, Vector2.zero, 0f, Vector2.one);
            anchor.SetFacingDirection(facingDirection);
            anchors.Add(anchor);
        }

        private SpriteRenderer GetOrCreateLayerRenderer(string layerName, int sortingOrder)
        {
            Transform existing = transform.Find(layerName);
            GameObject layerObject = existing != null ? existing.gameObject : new GameObject(layerName);
            layerObject.transform.SetParent(transform, false);
            layerObject.transform.localPosition = Vector3.zero;
            layerObject.transform.localRotation = Quaternion.identity;
            layerObject.transform.localScale = Vector3.one;

            SpriteRenderer renderer = layerObject.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = layerObject.AddComponent<SpriteRenderer>();
            }

            renderer.sortingOrder = sortingOrder;
            renderer.color = Color.white;
            return renderer;
        }
    }
}
