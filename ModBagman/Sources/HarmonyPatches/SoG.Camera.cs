using System.Reflection.Emit;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(Camera))]
static class SoG_Camera
{
    private static bool IsCameraInFreemode() => ModManager.ModBagman.IsCameraInFreemode;

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Camera.Update))]
    private static IEnumerable<CodeInstruction> FreemodeCamera(IEnumerable<CodeInstruction> code, ILGenerator gen)
    {
        var label = gen.DefineLabel();

        return new CodeMatcher(code)
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Camera), nameof(Camera.bEnableBoundsOverride)))
            )
            .ThrowIfInvalid("Balls")
            .Insert(
                new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => IsCameraInFreemode())),
                new CodeInstruction(OpCodes.Brfalse, label),
                new CodeInstruction(OpCodes.Ret),
                new CodeInstruction(OpCodes.Nop).WithLabels(label)
            )
            .InstructionEnumeration();
    }
}
