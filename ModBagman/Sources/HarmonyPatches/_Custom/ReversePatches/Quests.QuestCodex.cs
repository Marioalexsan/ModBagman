using Quests;

namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="QuestCodex"/>.
/// </summary>
[HarmonyPatch]
public static class Original_QuestCodex
{
    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(QuestCodex), nameof(QuestCodex.GetQuestDescription))]
    public static QuestDescription GetQuestDescription(QuestCodex.QuestID p_enID)
    {
        throw new NotImplementedException("Stub method.");
    }
}
