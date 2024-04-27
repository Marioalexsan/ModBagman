using Microsoft.Extensions.Logging;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), "ActuallyUnloadSFXBank")]
static class ActuallyUnloadSFXBank
{
    static void Prefix(string sName)
    {
        Program.Logger.LogInformation("Unloading sound wavebank " + sName);
    }
}
