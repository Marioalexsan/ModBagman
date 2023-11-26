using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="SoG.EnemyCodex"/>.
/// </summary>
[HarmonyPatch]
public static class Original_EnemyCodex
{
    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyDescription))]
    public static EnemyDescription GetEnemyDescription(EnemyCodex.EnemyTypes enType)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    //[HarmonyReversePatch]
    //[HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyDefaultAnimation))]
    public static Animation GetEnemyDefaultAnimation(EnemyCodex.EnemyTypes enType, ContentManager Content)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    //[HarmonyReversePatch]
    //[HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyDisplayIcon))]
    public static Texture2D GetEnemyDisplayIcon(EnemyCodex.EnemyTypes enType, ContentManager Content, bool bBigIfPossible)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    //[HarmonyReversePatch]
    //[HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyLocationPicture))]
    public static Texture2D GetEnemyLocationPicture(EnemyCodex.EnemyTypes enType, ContentManager Content)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyInstance_CacuteForward))]
    public static Enemy GetEnemyInstance_CacuteForward(EnemyCodex.EnemyTypes enType, Level.WorldRegion enOverrideContent)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyInstance))]
    public static Enemy GetEnemyInstance(EnemyCodex.EnemyTypes enType, Level.WorldRegion enOverrideContent)
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var original = AccessTools.Method(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyInstance_CacuteForward));
            var replacement = AccessTools.Method(typeof(Original_EnemyCodex), nameof(GetEnemyInstance_CacuteForward));

            return instructions.MethodReplacer(original, replacement);
        }

        _ = Transpiler(null);
        throw new NotImplementedException("Stub method.");
    }

}
