﻿namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(Game1), nameof(Game1._Saving_DeleteWorldFile))]
static class _Saving_DeleteWorldFile
{
    static void Postfix(int iFileSlot)
    {
        File.Delete(Path.Combine(Globals.AppDataPath, $"Worlds/{iFileSlot}.wld.gs"));
    }
}
