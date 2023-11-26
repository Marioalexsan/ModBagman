namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="SoG.CardCodex"/>.
/// </summary>
[HarmonyPatch]
public static class Original_CardCodex
{
    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(CardCodex), nameof(CardCodex.GetIllustrationPath))]
    public static string GetIllustrationPath(EnemyCodex.EnemyTypes enEnemy)
    {
        throw new NotImplementedException("Stub method.");
    }
}
