namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="FacegearCodex"/>.
/// </summary>
[HarmonyPatch]
public static class Original_FacegearCodex
{
    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(FacegearCodex), nameof(FacegearCodex.GetHatInfo))]
    public static FacegearInfo GetHatInfo(ItemCodex.ItemTypes enType)
    {
        throw new NotImplementedException("Stub method.");
    }
}
