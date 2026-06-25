using ProjectEclipse.Items;
using ProjectEclipse.Player;
using ProjectEclipse.Progression;
using UnityEngine;

namespace ProjectEclipse.Equipment
{
    public class EquipmentDefinition : ItemDefinition
    {
        [SerializeField] private EquipmentSlot slot = EquipmentSlot.Mainhand;
        [SerializeField] private EquipmentRarity rarity = EquipmentRarity.Common;
        [SerializeField] private EquipmentStats stats = new EquipmentStats();
        [SerializeField] private ClassRestriction classRestriction = new ClassRestriction();
        [SerializeField] private int legacyProgressionGateValue = 1;
        [SerializeField] private ResourceTier materialTierRequirement = ResourceTier.Wood;
        [SerializeField] private string routeRequirement;
        [SerializeField] private string unlockRequirement;
        [SerializeField] private Sprite visualSprite;
        [SerializeField] private EquippedVisualLayer visualLayer = EquippedVisualLayer.Mainhand;
        [SerializeField] private string equipmentType = "Gear";
        [SerializeField] private string specialEffectsPlaceholder;

        public EquipmentSlot Slot { get { return slot; } }
        public EquipmentRarity Rarity { get { return rarity; } }
        public EquipmentStats Stats { get { return stats; } }
        public ClassRestriction ClassRestriction { get { return classRestriction; } }
        public int LegacyProgressionGateValue { get { return Mathf.Max(1, legacyProgressionGateValue); } }
        public ResourceTier MaterialTierRequirement { get { return materialTierRequirement; } }
        public string RouteRequirement { get { return routeRequirement; } }
        public string UnlockRequirement { get { return unlockRequirement; } }
        public Sprite VisualSprite { get { return visualSprite != null ? visualSprite : WorldDropSprite; } }
        public bool HasExplicitVisualSprite { get { return visualSprite != null; } }
        public EquippedVisualLayer VisualLayer { get { return visualLayer; } }
        public string EquipmentType { get { return equipmentType; } }
        public string SpecialEffectsPlaceholder { get { return specialEffectsPlaceholder; } }

        public bool CanEquip(PlayerClassDefinition playerClass)
        {
            return classRestriction == null || classRestriction.Allows(playerClass);
        }

        public bool CanEquip(PlayerClassDefinition playerClass, int ignoredLegacyValue)
        {
            return CanEquip(playerClass);
        }

        protected void ConfigureEquipment(
            string id,
            string name,
            ItemCategory itemCategory,
            Color debugColor,
            ResourceTier tier,
            EquipmentSlot equipmentSlot,
            EquipmentRarity equipmentRarity,
            Sprite iconSprite = null,
            Sprite dropSprite = null,
            Sprite equippedSprite = null)
        {
            Configure(id, name, itemCategory, debugColor, 1, iconSprite, dropSprite, tier);
            slot = equipmentSlot;
            rarity = equipmentRarity;
            visualSprite = equippedSprite;
        }
    }
}
