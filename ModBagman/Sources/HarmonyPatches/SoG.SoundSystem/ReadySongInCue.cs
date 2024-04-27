using Microsoft.Xna.Framework.Audio;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.Extensions.Logging;
using System.Security.Policy;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), nameof(SoundSystem.ReadySongInCue))]
static class ReadySongInCue
{
    static void Prefix(ref string sSongName)
    {
        var redirects = AudioEntry.MusicRedirects;
        string audioIDToUse = sSongName;

        if (redirects.ContainsKey(audioIDToUse))
            audioIDToUse = redirects[audioIDToUse];

        Program.Logger.LogInformation($"Reading music in cue {audioIDToUse} ({sSongName})");
        sSongName = audioIDToUse;
    }

    // Also used by PlaySong Transpiler
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code, ILGenerator gen)
    {
        var codeList = code.ToList();

        // (local1 = GetMusicSoundBank(sCueName)) != null ? local1.GetCue(sCueName) : soundBank.GetCue(sCueName)

        Label skipVanillaBank = gen.DefineLabel();
        Label doVanillaBank = gen.DefineLabel();
        LocalBuilder modBank = gen.DeclareLocal(typeof(SoundBank));

        MethodInfo target = typeof(SoundBank).GetMethod(nameof(SoundBank.GetCue));

        var insertBefore = new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => _Helper.GetMusicSoundBank(null))),
            new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
            new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
            new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
            new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => _Helper.GetMusicName(null))),
            new CodeInstruction(OpCodes.Call, target),
            new CodeInstruction(OpCodes.Br, skipVanillaBank),
            new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
        };

        var insertAfter = new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
        };

        return codeList.InsertAroundMethod(target, insertBefore, insertAfter, editsReturnValue: true);
    }

}
