extern alias CompilerServices;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Quests;
using System.Text;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;

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

        if (Program.ReadConfig()?.GetValue("HarmonyDebug", false) ?? false)
            PrintAutoSplitDebugInfo();
    }

    /// <summary>
    /// This is just used to get some useful info for the AutoSplit script for speedrunning.
    /// </summary>
    static void PrintAutoSplitDebugInfo()
    {
        static int GetFieldOffset(FieldInfo fi) =>
                            (Marshal.ReadInt32(fi.FieldHandle.Value + (4 + IntPtr.Size)) & 0xFFFFFF) + IntPtr.Size;

        Dictionary<string, FieldInfo[]> paths = new()
        {
            ["inArcadeRun"] = new[]
            {
                AccessTools.Field(typeof(Game1), nameof(Game1.xGameSessionData)),
                AccessTools.Field(typeof(GameSessionData), nameof(GameSessionData.xRogueLikeSession)),
                AccessTools.Field(typeof(GameSessionData.RogueLikeSession), nameof(GameSessionData.RogueLikeSession.bInRun)),
            },
            ["arcadeFloor"] = new[]
            {
                AccessTools.Field(typeof(Game1), nameof(Game1.xGameSessionData)),
                AccessTools.Field(typeof(GameSessionData), nameof(GameSessionData.xRogueLikeSession)),
                AccessTools.Field(typeof(GameSessionData.RogueLikeSession), nameof(GameSessionData.RogueLikeSession.iCurrentFloor)),
            },
            ["gameMode"] = new[]
            {
                AccessTools.Field(typeof(Game1), nameof(Game1.xStateMaster)),
                AccessTools.Field(typeof(StateMaster), nameof(StateMaster.enGameMode)),
            },
            ["gameState"] = new[]
            {
                AccessTools.Field(typeof(Game1), nameof(Game1.xStateMaster)),
                AccessTools.Field(typeof(StateMaster), nameof(StateMaster.enGameState)),
            },
            ["zoningState"] = new[]
            {
                AccessTools.Field(typeof(Game1), nameof(Game1.xLevelMaster)),
                AccessTools.Field(typeof(LevelMaster), nameof(LevelMaster.xZoningHelper)),
                AccessTools.Field(typeof(LevelMaster.ZoningHelper), nameof(LevelMaster.ZoningHelper.iZoningStateProgress)),
            },
            ["inCutscene"] = new[]
            {
                AccessTools.Field(typeof(Game1), nameof(Game1.xCutsceneMaster)),
                AccessTools.Field(typeof(CutsceneControl), nameof(CutsceneControl.bInCutscene)),
            },
            ["currentCutscene"] = new[]
            {
                AccessTools.Field(typeof(Game1), nameof(Game1.xCutsceneMaster)),
                AccessTools.Field(typeof(CutsceneControl), nameof(CutsceneControl.xActiveCutscene)),
                AccessTools.Field(typeof(Cutscene), nameof(Cutscene.enID)),
            },
            ["flagCount"] = new[]
            {
                AccessTools.Field(typeof(Game1), nameof(Game1.xGameSessionData)),
                AccessTools.Field(typeof(GameSessionData), nameof(GameSessionData.henActiveFlags)),
                AccessTools.Field(typeof(HashSet<FlagCodex.FlagID>), "m_count"),
            },
            ["questList"] = new[]
            {
                AccessTools.Field(typeof(Game1), nameof(Game1.xLocalPlayer)),
                AccessTools.Field(typeof(PlayerView), nameof(PlayerView.xJournalInfo)),
                AccessTools.Field(typeof(Journal), nameof(Journal.xQuestLog)),
                AccessTools.Field(typeof(QuestLog), nameof(QuestLog.lxActiveQuests)),
            },
            ["questDoneList"] = new[]
            {
                AccessTools.Field(typeof(Game1), nameof(Game1.xLocalPlayer)),
                AccessTools.Field(typeof(PlayerView), nameof(PlayerView.xJournalInfo)),
                AccessTools.Field(typeof(Journal), nameof(Journal.xQuestLog)),
                AccessTools.Field(typeof(QuestLog), nameof(QuestLog.lxCompletedQuests)),
            },
            ["gameSession"] = new[]
            {
                AccessTools.Field(typeof(Game1), nameof(Game1.xGameSessionData)),
            },
            ["HashSet<FlagCodex.FlagID>.m_count"] = new[]
            {
                AccessTools.Field(typeof(HashSet<FlagCodex.FlagID>), "m_count"),
            },
            ["HashSet<FlagCodex.FlagID>.m_slots"] = new[]
            {
                AccessTools.Field(typeof(HashSet<FlagCodex.FlagID>), "m_slots"),
            },
            ["List<Quest>._size"] = new[]
            {
                AccessTools.Field(typeof(List<Quest>), "_size"),
            },
            ["List<Quest>._items"] = new[]
            {
                AccessTools.Field(typeof(List<Quest>), "_items"),
            },
            ["Quest.enQuestID"] = new[]
            {
                AccessTools.Field(typeof(Quest), nameof(Quest.enQuestID)),
            },
            ["Quest.lxObjectives"] = new[]
            {
                AccessTools.Field(typeof(Quest), nameof(Quest.lxObjectives)),
            },
            ["QuestObjective.bFinished"] = new[]
            {
                AccessTools.Field(typeof(QuestObjective), nameof(QuestObjective.bFinished)),
            }
        };

        Program.Logger.LogInformation("Logging offsets...");
        foreach (var path in paths)
        {
            StringBuilder builder = new();

            builder.Append(path.Key).Append(": ");

            foreach (var field in path.Value)
            {
                builder.Append("0x");
                builder.Append(GetFieldOffset(field).ToString("X"));
                builder.Append(" => ");
            }

            if (path.Value.Length > 0)
                builder.Length -= " => ".Length;

            Program.Logger.LogInformation("{}", builder.ToString());
        }
        Program.Logger.LogInformation("Offset logging done.");
    }
}
