using static SoG.ShopMenu;

namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="TreatCurseMenu"/>.
/// </summary>
[HarmonyPatch]
public static class Original_ShopMenu_TreatCurseMenu
{
    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(TreatCurseMenu), nameof(TreatCurseMenu.FillCurseList))]
    public static void FillCurseList(TreatCurseMenu __instance)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(TreatCurseMenu), nameof(TreatCurseMenu.FillTreatList))]
    public static void FillTreatList(TreatCurseMenu __instance)
    {
        throw new NotImplementedException("Stub method.");
    }
}
