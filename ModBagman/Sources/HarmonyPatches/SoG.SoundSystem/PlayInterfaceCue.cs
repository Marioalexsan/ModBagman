using System.Reflection.Emit;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), nameof(SoundSystem.PlayInterfaceCue))]
static class PlayInterfaceCue
{
    static void Prefix(ref string sCueName)
    {
        var redirects = AudioEntry.EffectRedirects;
        string audioIDToUse = sCueName;

        if (redirects.ContainsKey(audioIDToUse))
            audioIDToUse = redirects[audioIDToUse];

        sCueName = audioIDToUse;
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code, ILGenerator gen)
        => PlayCue.Transpiler(code, gen);
}
