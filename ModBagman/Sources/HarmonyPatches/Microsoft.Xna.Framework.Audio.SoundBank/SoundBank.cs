using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Audio;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch]
internal static class Microsoft_Xna_Framework_Audio_SoundBank
{
    [HarmonyPatch(typeof(SoundBank), nameof(SoundBank.GetCue))]
    [HarmonyFinalizer]
    static void LogCrashedCue(string name, Exception __exception)
    {
        if (__exception != null)
            Program.Logger.LogInformation("XNA GetCue failed: " + name);
    }
}
