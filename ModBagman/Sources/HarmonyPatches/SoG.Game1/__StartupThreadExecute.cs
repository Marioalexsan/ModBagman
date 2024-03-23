extern alias CompilerServices; 
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Quests;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(Game1), nameof(Game1.__StartupThreadExecute))]
static class __StartupThreadExecute
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code, ILGenerator gen)
    {
        var codeList = code.ToList();

        MethodInfo target = typeof(DialogueCharacterLoading).GetMethod("Init");

        MethodInfo targetTwo = typeof(Game1).GetMethod(nameof(Game1._Loading_LoadGlobalFile));

        var insert = new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => PrepareModLoader()))
        };

        var moreInsert = new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => Globals.UpdateVersionNumber()))
        };

        return codeList
            .InsertAfterMethod(target, insert)
            .InsertBeforeMethod(targetTwo, moreInsert);
    }

    static void PrepareModLoader()
    {
        GameObjectStuff.Load();
        ModBagmanResources.ReloadResources();
        ModManager.Reload();

        EventInput.EventInput.CharEntered += (o, e) => Globals.Console?.CharEntered(o, e);
        EventInput.EventInput.KeyDown += (o, e) => Globals.Console?.KeyDown(o, e);

        MainMenuWorker.UpdateStorySaveCompatibility();
        MainMenuWorker.UpdateArcadeSaveCompatibility();

        PrintAutoSplitDebugInfo();
    }

    /// <summary>
    /// This is just used to get some useful info for the AutoSplit script for speedrunning.
    /// </summary>
    static void PrintAutoSplitDebugInfo()
    {
        static int GetFieldOffset(FieldInfo fi) =>
                            Marshal.ReadInt32(fi.FieldHandle.Value + (4 + IntPtr.Size)) & 0xFFFFFF;

        var fields = new[]
        {
            // Things we go through
            AccessTools.Field(typeof(Game1), nameof(Game1.xGameSessionData)),
            AccessTools.Field(typeof(Game1), nameof(Game1.xStateMaster)),
            AccessTools.Field(typeof(Game1), nameof(Game1.xCutsceneMaster)),
            AccessTools.Field(typeof(Game1), nameof(Game1.xLocalPlayer)),
            AccessTools.Field(typeof(Game1), nameof(Game1.xLevelMaster)),
            AccessTools.Field(typeof(GameSessionData), nameof(GameSessionData.xRogueLikeSession)),
            AccessTools.Field(typeof(LevelMaster), nameof(LevelMaster.xZoningHelper)),
            AccessTools.Field(typeof(PlayerView), nameof(PlayerView.xJournalInfo)),
            AccessTools.Field(typeof(Journal), nameof(Journal.xQuestLog)),
            AccessTools.Field(typeof(CutsceneControl), nameof(CutsceneControl.xActiveCutscene)),

            // The actual relevant stuff as seen in the ASL script
            AccessTools.Field(typeof(GameSessionData.RogueLikeSession), nameof(GameSessionData.RogueLikeSession.bInRun)),
            AccessTools.Field(typeof(GameSessionData.RogueLikeSession), nameof(GameSessionData.RogueLikeSession.iCurrentFloor)),
            AccessTools.Field(typeof(StateMaster), nameof(StateMaster.enGameMode)),
            AccessTools.Field(typeof(StateMaster), nameof(StateMaster.enGameState)),
            AccessTools.Field(typeof(LevelMaster.ZoningHelper), nameof(LevelMaster.ZoningHelper.iZoningStateProgress)),
            AccessTools.Field(typeof(CutsceneControl), nameof(CutsceneControl.bInCutscene)),
            AccessTools.Field(typeof(Cutscene), nameof(Cutscene.enID)),
            AccessTools.Field(typeof(QuestLog), nameof(QuestLog.lxActiveQuests)),
            AccessTools.Field(typeof(QuestLog), nameof(QuestLog.lxCompletedQuests)),
            AccessTools.Field(typeof(Game1), nameof(Game1.xGameSessionData)),
        };

        Program.Logger.LogInformation("Logging offsets...");
        foreach (var field in fields)
        {
            Program.Logger.LogInformation("{} -> {} = {}", field.DeclaringType.FullName, field.Name, (GetFieldOffset(field) + IntPtr.Size).ToString("X"));
        }
        Program.Logger.LogInformation("Offset logging done.");
    }
}
