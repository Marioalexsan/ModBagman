using CommandLine;

namespace ModBagman;

internal class CLIOptions
{
    [Option(HelpText = "Print the mod tool version.")]
    public bool Version { get; set; }

    [Option("gen-d-ts", Required = false, HelpText = "Specify to generate TypeScript definitions for the JavaScript modding API.")]
    public bool GenerateTypeScriptDefinitons { get; set; }
}
