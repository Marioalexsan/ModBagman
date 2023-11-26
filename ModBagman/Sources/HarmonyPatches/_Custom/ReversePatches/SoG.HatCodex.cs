namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="HatCodex"/>.
/// </summary>
[HarmonyPatch]
public static class Original_HatCodex
{
    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(HatCodex), nameof(HatCodex.GetHatInfo))]
    public static HatInfo GetHatInfo(ItemCodex.ItemTypes enType)
    {
        throw new NotImplementedException("Stub method.");
    }
}
