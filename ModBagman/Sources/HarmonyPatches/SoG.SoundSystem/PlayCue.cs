using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem))]
static class PlayCue
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(SoundSystem), nameof(SoundSystem.PlayCue), new[] { typeof(string), typeof(Vector2) });
        yield return AccessTools.Method(typeof(SoundSystem), nameof(SoundSystem.PlayCue), new[] { typeof(string), typeof(TransformComponent) });
        yield return AccessTools.Method(typeof(SoundSystem), nameof(SoundSystem.PlayCue), new[] { typeof(string), typeof(Vector2), typeof(float) });
        yield return AccessTools.Method(typeof(SoundSystem), nameof(SoundSystem.PlayCue), new[] { typeof(string), typeof(IEntity), typeof(bool), typeof(bool) });
    }

    static void Finalizer(string sCueName, Exception __exception)
    {
        if (__exception != null)
            Program.Logger.LogError($"PlayCue crashed while trying to play {sCueName} : {_Helper.GetEffectName(sCueName)}");
    }

    static void Prefix(ref string sCueName)
    {
        var redirects = AudioEntry.EffectRedirects;
        string audioIDToUse = sCueName;

        if (redirects.ContainsKey(audioIDToUse))
            audioIDToUse = redirects[audioIDToUse];

        sCueName = audioIDToUse;
    }

    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code, ILGenerator gen)
    {
        var codeList = code.ToList();

        // Original: soundBank.GetCue(sCueName)
        // Modified: (local1 = GetEffectSoundBank(sCueName)) != null ? local1.GetCue(sCueName) : soundBank.GetCue(sCueName)

        Label skipVanillaBank = gen.DefineLabel();
        Label doVanillaBank = gen.DefineLabel();
        LocalBuilder modBank = gen.DeclareLocal(typeof(SoundBank));

        MethodInfo target = AccessTools.Method(typeof(SoundBank), nameof(SoundBank.GetCue));

        var insertBefore = new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => _Helper.GetEffectSoundBank(null))),
            new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
            new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
            new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
            new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => _Helper.GetEffectName(null))),
            new CodeInstruction(OpCodes.Call, target),
            new CodeInstruction(OpCodes.Br, skipVanillaBank),
            new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
        };

        var insertAfter = new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
        };

        return codeList
            .InsertAroundMethod(target, insertBefore, insertAfter, editsReturnValue: true, methodIndex: 1)
            .InsertAroundMethod(target, insertBefore, insertAfter, editsReturnValue: true);
    }
}
