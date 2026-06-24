using UnityEngine;

namespace ProjectEclipse.Crafting
{
    public static class CraftingTrinketTierUtility
    {
        public static CraftingTrinketTier FromNumericTier(int tier)
        {
            int index = Mathf.Clamp(Mathf.Max(1, tier) - 1, 0, (int)CraftingTrinketTier.Origin);
            return (CraftingTrinketTier)index;
        }

        public static string FormatTier(int tier)
        {
            return FromNumericTier(tier) + " (" + Mathf.Max(1, tier) + ")";
        }
    }
}
