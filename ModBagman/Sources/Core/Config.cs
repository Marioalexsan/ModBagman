namespace ModBagman;

internal class Config
{
    /// <summary>
    /// Locks the config, preventing it from being modified in any way.
    /// </summary>
    public bool ConfigReadonly { get; set; } = false;

    /// <summary>
    /// List of files names that should be skipped from being loaded as mods.
    /// </summary>
    public List<string> IgnoredMods { get; set; } = new();

    /// <summary>
    /// Specifies the value that will be set for <see cref="Harmony.DEBUG"/>
    /// </summary>
    public bool HarmonyDebug { get; set; } = false;

    /// <summary>
    /// If true, logs verbose information related to Harmony patches.
    /// </summary>
    public bool VerbosePatchingLog { get; set; } = false;

    /// <summary>
    /// If true, logs some utility offsets for autosplitter development purposes.
    /// </summary>
    public bool PrintAutoSplitOffsets { get; set; } = false;

    /// <summary>
    /// Paths to additional folders to search for loadable mods.
    /// </summary>
    public List<string> ExtraModFolders { get; set; } = new();

    /// <summary>
    /// Paths to additional files to consider for loading as mods.
    /// </summary>
    public List<string> ExtraModPaths { get; set; } = new();
}
