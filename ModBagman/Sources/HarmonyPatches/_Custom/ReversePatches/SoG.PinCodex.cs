namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="PinCodex"/>.
/// </summary>
[HarmonyPatch]
public static class Original_PinCodex
{
    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(PinCodex), nameof(PinCodex.GetInfo))]
    public static PinInfo GetInfo(RogueLikeMode.Perks enType)
    {
        throw new NotImplementedException("Stub method.");
    }
}
