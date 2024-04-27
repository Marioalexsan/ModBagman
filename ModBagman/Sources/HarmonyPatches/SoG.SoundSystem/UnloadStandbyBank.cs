using Microsoft.Extensions.Logging;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), nameof(SoundSystem.UnloadStandbyBank))]
static class UnloadStandbyBank
{
    static void Prefix(string sStandbyBank)
    {
        Program.Logger.LogInformation("Unloading music wavebank " + sStandbyBank);
    }
}
