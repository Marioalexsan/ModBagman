using Microsoft.Xna.Framework.Graphics;

namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="SoG.Game1"/>.
/// </summary>
[HarmonyPatch]
public static class Original_Game1
{
    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Game1), nameof(Game1._RogueLike_GetTreatCurseInfo))]
    public static void _RogueLike_GetTreatCurseInfo(Game1 __instance, RogueLikeMode.TreatsCurses enTreatCurse, out string sNameHandle, out string sDescriptionHandle, out float fScoreModifier)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Game1), nameof(Game1._LevelLoading_DoStuff))]
    public static void _LevelLoading_DoStuff(Game1 __instance, Level.ZoneEnum enLevel, bool bStaticOnly)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Game1), nameof(Game1._RogueLike_GetPerkTexture))]
    public static Texture2D _RogueLike_GetPerkTexture(Game1 __instance, RogueLikeMode.Perks enPerk)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Game1), nameof(Game1._Enemy_AdjustForDifficulty))]
    public static void _Enemy_AdjustForDifficulty(Game1 __instance, Enemy xEn)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Game1), nameof(Game1._Enemy_MakeElite))]
    public static bool _Enemy_MakeElite(Game1 __instance, Enemy xEn, bool bAttachEffect)
    {
        throw new NotImplementedException("Stub method.");
    }
}
