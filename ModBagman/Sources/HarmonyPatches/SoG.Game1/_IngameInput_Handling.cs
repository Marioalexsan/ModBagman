namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(Game1), nameof(Game1._IngameInput_Handling))]
static class _IngameInput_Handling
{
    static void Postfix()
    {
        Globals.Console?.Update();
    }
}
