namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="RogueLikeMode.PerkInfo"/>.
/// </summary>
[HarmonyPatch]
public static class Original_RoguelikeMode_PerkInfo
{
    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RogueLikeMode.PerkInfo), nameof(RogueLikeMode.PerkInfo.Init))]
    public static void Init()
    {
        throw new NotImplementedException("Stub method.");
    }
}
