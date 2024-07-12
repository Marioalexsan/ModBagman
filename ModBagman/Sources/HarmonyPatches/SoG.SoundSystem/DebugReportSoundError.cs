using Microsoft.Extensions.Logging;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), nameof(SoundSystem.DebugReportSoundError))]
static class DebugReportSoundError
{
    static bool Prefix(string sCueName)
    {
        Program.Logger.LogInformation("Failed to play cue " + sCueName + ", trying to play \"Silence\" instead.");
        return false;
    }
}
