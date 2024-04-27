using System.Text.Json;

namespace ModBagman;

internal class Config
{
    public bool ConfigReadonly { get; set; } = false;

    public List<string> IgnoredMods { get; set; } = new();

    public bool HarmonyDebug { get; set; } = false;

    public bool VerbosePatchingLog { get; set; } = false;

    public bool PrintAutoSplitOffsets { get; set; } = false;

    public List<string> ExtraModFolders { get; set; } = new();

    public List<string> ExtraModPaths { get; set; } = new();
}
