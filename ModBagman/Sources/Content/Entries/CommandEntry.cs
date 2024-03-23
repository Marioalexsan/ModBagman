using CommandLine;
using Microsoft.Extensions.Logging;
using MonoMod.Utils;
using System.Reflection;

namespace ModBagman;

/// <summary> 
/// Delegate that parses a chat command. 
/// </summary>
/// <param name="args"> The argument list. </param>
/// <param name="connection"> The connection identifier of the player. </param>
public delegate void CommandParser(string[] args, long connection);

/// <summary>
/// Defines custom commands that can be entered from the in-game chat. <para/>
/// All modded commands are called by using the "/{<see cref="Mod.Name"/>}:{Command} [args] format. <para/>
/// For instance, you can use "/ModBagman:Help" to invoke the mod tool's help command.
/// </summary>
/// <remarks> 
/// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
/// </remarks>
[ModEntry(0)]
public class CommandEntry : Entry<CustomEntryID.CommandID>
{
    internal CommandEntry() { }

    public Dictionary<string, CommandParser> Commands = new();

    public Dictionary<string, string> HelpText = new();

    /// <summary>
    /// Name alias to use for this mod.
    /// If set to a value other than null or undefined, the mod commands will 
    /// also be callable using the "/(Alias):(Command) (args)" format.
    /// If multiple mods set the same alias, only one of them will be able to use the alias.
    /// Ideally, you want this to be a short and easy to use name.
    /// </summary>
    public string Alias { get; set; }

    public void AddCommand(string command, Delegate action, string description = "")
    {
        CommandParser parser = null;

        if (typeof(CommandParser).IsAssignableFrom(action.GetType()))
        {
            parser = (CommandParser)action;
        }
        else if (typeof(Action<string[], int>).IsAssignableFrom(action.GetType()))
        {
            var converted = (Action<string[], int>)action;
            parser = (args, connection) => converted(args, (int)connection);
        }
        else if (typeof(Action<string[]>).IsAssignableFrom(action.GetType()))
        {
            var converted = (Action<string[]>)action;
            parser = (args, connection) => converted(args);
        }
        else if (typeof(Action).IsAssignableFrom(action.GetType()))
        {
            var converted = (Action)action;
            parser = (args, connection) => converted();
        }

        Commands[command] = parser ?? throw new ArgumentException("The given action isn't convertible to a command.");
        HelpText[command] = description;
    }

    public void AutoAddModCommands(string alias = null)
    {
        if (Mod is JavaScriptMod)
            throw new InvalidOperationException("Auto adding mod commands is possible only for C# mods.");

        var methods = AccessTools.GetDeclaredMethods(Mod.GetType())
            .Select(x => (method: x, attrib: x.GetCustomAttribute<ModCommandAttribute>()))
            .Where(x => x.attrib != null);

        foreach (var (method, attrib) in methods)
        {
            bool success = false;

            try
            {
                AddCommand(attrib.Command, method.CreateDelegate(typeof(CommandParser), method.IsStatic ? null : Mod), attrib.Description);
                success = true;
            }
            catch { }

            if (!success)
            {
                try
                {
                    AddCommand(attrib.Command, method.CreateDelegate(typeof(Action<string[], int>), method.IsStatic ? null : Mod), attrib.Description);
                    success = true;
                }
                catch { }
            }

            if (!success)
            {
                try
                {
                    AddCommand(attrib.Command, method.CreateDelegate(typeof(Action<string[]>), method.IsStatic ? null : Mod), attrib.Description);
                    success = true;
                }
                catch { }
            }

            if (!success)
            {
                try
                {
                    AddCommand(attrib.Command, method.CreateDelegate(typeof(Action), method.IsStatic ? null : Mod), attrib.Description);
                    success = true;
                }
                catch { }
            }

            if (!success)
            {
                Program.Logger.LogWarning($"Couldn't add command {attrib.Command}: The given method isn't an acceptable type for a mod command.");
            }
        }

        Alias = alias;
    }

    internal static Mod MatchModByTarget(string target)
    {
        static bool FuzzyMatch(string name, string target) => string.Equals(name, target, StringComparison.InvariantCultureIgnoreCase);

        return ModManager.Mods.FirstOrDefault(x => x.Name == target)
            ?? ModManager.Mods.FirstOrDefault(x => FuzzyMatch(x.Name, target))
            ?? Entries.Commands.FirstOrDefault(x => x.Alias == target)?.Mod
            ?? Entries.Commands.FirstOrDefault(x => FuzzyMatch(x.Alias, target))?.Mod;
    }

    internal override void Initialize()
    {
        // Nothing to do
    }

    internal override void Cleanup()
    {
        // Nothing to do
    }
}
