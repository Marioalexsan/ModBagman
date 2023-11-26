using System.Reflection.Emit;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), nameof(SoundSystem.PlayTrackableInterfaceCue))]
static class PlayTrackableInterfaceCue
{
    static void Prefix(ref string sCueName)
    {
        var redirects = AudioEntry.EffectRedirects;
        string audioIDToUse = sCueName;

        if (redirects.ContainsKey(audioIDToUse))
            audioIDToUse = redirects[audioIDToUse];

        sCueName = audioIDToUse;
    }
    
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code, ILGenerator gen)
        => PlayCue.Transpiler(code, gen);
}
