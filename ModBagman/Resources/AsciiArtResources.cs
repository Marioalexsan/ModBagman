namespace ModBagman;

internal static class AsciiArtResources
{
    public const string AlertNotice = """
                ┌────────────────────────────────────────────────────────────────────────┐
                │    ________     ___         _______      ________     __________       │
                │    \   __  \    \  \        \  ____\     \   __  \    \___   ___\      │
                │     \  \_\  \    \  \        \  \____     \  \_\  \       \  \         │
                │      \   __  \    \  \        \   ___\     \   _  |        \  \        │
                │       \  \ \  \    \  \____    \  \_____    \  \\  \        \  \       │
                │        \__\ \__\    \_______\   \_______\    \__\\__\        \__\      │
                │                                                                        │
                └────────────────────────────────────────────────────────────────────────┘
                """;

    public const string CopySavesNotice = """
                ┌────────────────────────────────────────────────────────────────────────┐
                │                                                                        │
                │      Seems like this is the first time you're using ModBagman!         │
                │      The mod tool uses a separate save location from vanilla SoG.      │
                │      Would you like to copy over your saves from the base game?        │
                │                                                                        │
                │      SoG savepath:       %appdata%\Secrets of Grindea\                 │
                │      ModBagman savepath: %appdata%\ModBagman\                          │
                │                                                                        │
                │                                                                        │
                │                  [Y] Hell yeah!    [N] Nah, don't.                     │
                └────────────────────────────────────────────────────────────────────────┘
                """;

    public const string SavesCopiedSuccessfullyNotice = """
                ┌────────────────────────────────────────────────────────────────────────┐
                │                                                                        │
                │                            Saves copied!                               │
                │                                                                        │
                └────────────────────────────────────────────────────────────────────────┘
                """;

}
