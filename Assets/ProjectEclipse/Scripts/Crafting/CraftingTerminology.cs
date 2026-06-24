namespace ProjectEclipse.Crafting
{
    public static class CraftingTerminology
    {
        public const string TrinketSingular = "Crafting Trinket";
        public const string TrinketPlural = "Crafting Trinkets";
        public const string Handcrafted = "Handcrafted";

        public static string GetStationDisplayName(CraftingStationType stationType)
        {
            switch (stationType)
            {
                case CraftingStationType.Inventory:
                    return Handcrafted;
                case CraftingStationType.FurnacePort:
                    return "Basic Furnace Trinket";
                case CraftingStationType.CauldronPort:
                    return "Basic Cauldron Trinket";
                case CraftingStationType.ForgePort:
                    return "Basic Forge Trinket";
                case CraftingStationType.AnvilPort:
                    return "Basic Anvil Trinket";
                case CraftingStationType.UtilityPort:
                    return "Basic Woodworking Trinket";
                case CraftingStationType.WorkbenchPort:
                    return "Basic Workbench Trinket";
                case CraftingStationType.ArcanePort:
                    return "Arcane Trinket";
                case CraftingStationType.LeatherworkingPort:
                    return "Leatherworking Trinket";
                case CraftingStationType.LoomPort:
                    return "Loom Trinket";
                case CraftingStationType.GemcuttingPort:
                    return "Gemcutting Trinket";
                case CraftingStationType.RunesmithPort:
                    return "Runesmith Trinket";
                default:
                    return stationType.ToString();
            }
        }

        public static string GetSlotDisplayName(CraftingPortSlot slot)
        {
            switch (slot)
            {
                case CraftingPortSlot.FurnacePort:
                    return "Furnace";
                case CraftingPortSlot.CauldronPort:
                    return "Cauldron";
                case CraftingPortSlot.ForgePort:
                    return "Forge";
                case CraftingPortSlot.AnvilPort:
                    return "Anvil";
                case CraftingPortSlot.UtilityPort:
                    return "Woodworking";
                case CraftingPortSlot.WorkbenchPort:
                    return "Workbench";
                case CraftingPortSlot.ArcanePort:
                    return "Arcane";
                case CraftingPortSlot.LeatherworkingPort:
                    return "Leatherworking";
                case CraftingPortSlot.LoomPort:
                    return "Loom";
                case CraftingPortSlot.GemcuttingPort:
                    return "Gemcutting";
                case CraftingPortSlot.RunesmithPort:
                    return "Runesmith";
                default:
                    return slot.ToString();
            }
        }
    }
}
