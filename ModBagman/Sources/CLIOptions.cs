using CommandLine;

namespace ModBagman;

internal class CLIOptions
{
    [Option(HelpText = "Print the mod tool version.")]
    public bool Version { get; set; }
}
