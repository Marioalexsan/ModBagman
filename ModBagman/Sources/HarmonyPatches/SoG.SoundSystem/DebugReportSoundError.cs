using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Audio;

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
