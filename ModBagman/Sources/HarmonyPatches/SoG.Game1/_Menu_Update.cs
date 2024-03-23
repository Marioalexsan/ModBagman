using System.Reflection.Emit;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(Game1), nameof(Game1._Menu_Update))]
static class _Menu_Update
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
    {
        List<CodeInstruction> code = instructions.ToList();

        var label = gen.DefineLabel();

        var inserted = new List<CodeInstruction>
        {
            CodeInstruction.Call(() => IsDevConsoleActive()),
            new CodeInstruction(OpCodes.Brfalse, label),
            new CodeInstruction(OpCodes.Ret),
            new CodeInstruction(OpCodes.Nop).WithLabels(label)
        };

        return code.InsertAfterMethod(AccessTools.Method(typeof(LocalInputHelper), nameof(LocalInputHelper.Update)), inserted);
    }

    static void Postfix()
    {
        Globals.Console?.Update();
        MainMenuWorker.MenuUpdate();
    }

    static bool IsDevConsoleActive() => Globals.Console.Active;
}
