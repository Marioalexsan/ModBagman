namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(CAS))]
static class SoG_CAS
{
    internal static bool RedirectChatToConsole { get; set; }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CAS.AddChatMessage))]
    static bool AddChatMessage_Prefix(string sMessage)
    {
        if (RedirectChatToConsole)
        {
            Globals.Console?.AddMessage(sMessage);
        }

        return !RedirectChatToConsole;
    }
}