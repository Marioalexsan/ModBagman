using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Audio;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), nameof(SoundSystem.HandleError))]
static class HandleError
{
    static int CrashesSoFar = 0;

    static bool Prefix(Exception e)
    {
        if (CrashesSoFar >= 60)
            return false;

        Program.Logger.LogError("Crashed while getting music cue: " + e.ToString());
        CrashesSoFar++;

        if (CrashesSoFar == 60)
            Program.Logger.LogError("Reached max errors from crashed music, will stop logging this.");

        return false;
    }
}
