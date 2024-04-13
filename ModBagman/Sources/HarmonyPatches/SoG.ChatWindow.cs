namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(ChatWindow))]
static class SoG_ChatWindow
{
    internal static bool RedirectChatToConsole { get; set; }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ChatWindow.AddMessage), new[] { typeof(string) })]
    static bool AddMessage0_Prefix(string p_sMessage)
    {
        if (RedirectChatToConsole)
        {
            Globals.Console?.AddMessage(p_sMessage);
        }

        return !RedirectChatToConsole;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ChatWindow.AddMessage), new[] { typeof(ChatMessage), typeof(bool) })]
    static bool AddMessage1_Prefix(ChatMessage p_xMessage, bool bNoSpam)
    {
        if (RedirectChatToConsole)
        {
            Globals.Console?.AddMessage(p_xMessage.sMessage);
        }

        return !RedirectChatToConsole;
    }
}