using System.Reflection.Emit;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(Game1), nameof(Game1._Chat_DistributeChatMessage))]
class _Chat_DistributeChatMessage
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code)
    {
        return new CodeMatcher(code)
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Ldarg_2),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Game1), nameof(Game1._Chat_ParseCommand))),
                new CodeMatch(OpCodes.Ret)
            )
            .RemoveInstructions(7)
            .Instructions();
    }
}
