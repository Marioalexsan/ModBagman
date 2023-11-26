namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="SpellCodex"/>.
/// </summary>
[HarmonyPatch]
public static class Original_SpellCodex
{
    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(SpellCodex), nameof(SpellCodex.IsMagicSkill))]
    public static bool IsMagicSkill(SpellCodex.SpellTypes enType)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(SpellCodex), nameof(SpellCodex.IsMeleeSkill))]
    public static bool IsMeleeSkill(SpellCodex.SpellTypes enType)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(SpellCodex), nameof(SpellCodex.IsUtilitySkill))]
    public static bool IsUtilitySkill(SpellCodex.SpellTypes enType)
    {
        throw new NotImplementedException("Stub method.");
    }
}
