namespace ProjectEclipse.Crafting
{
    public enum CraftingRequirementStatus
    {
        None,
        Satisfied,
        Reserved,
        Queueable,
        Processing,
        Missing,
        MissingPort,
        InsufficientPortTier,
        RecipeLocked,
        Complete
    }
}
