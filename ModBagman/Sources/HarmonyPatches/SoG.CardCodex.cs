namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(CardCodex))]
static class SoG_CardCodex
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CardCodex.GetIllustrationPath))]
    static bool GetIllustrationPath_Prefix(ref string __result, EnemyCodex.EnemyTypes enEnemy)
    {
        var entry = Entries.Enemies.GetRequired(enEnemy);

        if (entry.CardIllustrationPath == null && entry.IsVanilla)
            return true;

        __result = entry.CardIllustrationPath;
        return false;
    }
}
