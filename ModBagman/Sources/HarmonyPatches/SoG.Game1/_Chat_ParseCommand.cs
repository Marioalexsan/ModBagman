using System.Reflection.Emit;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(Game1), nameof(Game1._Chat_ParseCommand))]
static class _Chat_ParseCommand
{
    internal static void ParseModCommands(string command, string message, long connection)
    {
        Program.Logger.LogInformation($"Calling with {command} {message}");
        string[] words = command.Split(new[] { ':' }, 2);

        if (words.Length != 2)
        {
            CAS.AddChatMessage($"[{ModBagmanMod.ModName}] Command syntax: /<mod>:<command> [args...].");
            CAS.AddChatMessage($"[{ModBagmanMod.ModName}] Use vanilla commands with /sog:<command> [args...]");
            return;
        }

        string target = words[0];
        string trueCommand = words[1];

        /* Try in order:
         * 1. Direct matches by name
         * 2. Fuzzy matches by name
         * 3. Direct matches by alias
         * 4. Fuzzy matches by alias
         */
        Mod mod = CommandEntry.MatchModByTarget(target);

        if (mod == null)
        {
            CAS.AddChatMessage($"[{ModBagmanMod.ModName}] Unknown mod '{target}'!");
            return;
        }

        if (mod == ModManager.Vanilla)
        {
            // Intercept vanilla calls
            if (trueCommand.Equals("help", StringComparison.InvariantCultureIgnoreCase))
            {
                if (message != "")
                {
                    Program.Logger.LogInformation($"Parsed help command for {target}");
                    ParseModCommands($"{ModBagmanMod.ModName}:Help", $"{target}", connection);
                    return;
                }
                else
                {
                    Program.Logger.LogInformation($"Parsed help command for {target}, arguments: {message}");
                    ParseModCommands($"{ModBagmanMod.ModName}:Help", $"{target}:{message}", connection);
                    return;
                }
            }
            else
            {
                // Return the modified command
                Program.Logger.LogInformation($"Running vanilla command {trueCommand}: {message}");
                Globals.Game._Chat_ParseCommand(trueCommand + " " + message, connection);
                return;
            }
        }

        var entry = Entries.Commands.Get(mod, "");

        CommandParser parser = null;

        if (entry != null && !entry.Commands.TryGetValue(trueCommand, out parser))
        {
            // Try case insensitive close matches
            parser = entry.Commands.FirstOrDefault(x => x.Key.Equals(trueCommand, StringComparison.InvariantCultureIgnoreCase)).Value;
        }

        if (entry == null || parser == null)
        {
            Program.Logger.LogInformation($"entry {entry != null}, parser ${parser != null}");

            if (trueCommand.Equals("help", StringComparison.InvariantCultureIgnoreCase))
            {
                if (message != "")
                {
                    Program.Logger.LogInformation($"Parsed help command for {target}");
                    ParseModCommands($"{ModBagmanMod.ModName}:Help", $"{target}", connection);
                    return;
                }
                else
                {
                    Program.Logger.LogInformation($"Parsed help command for {target}, arguments: {message}");
                    ParseModCommands($"{ModBagmanMod.ModName}:Help", $"{target}:{message}", connection);
                    return;
                }
            }

            CAS.AddChatMessage($"[{target}] Unknown command!");
            return;
        }

        string[] args = message.Split(new char[] { ' ' }, options: StringSplitOptions.RemoveEmptyEntries);

        Program.Logger.LogInformation("Parsed mod command {target} : {trueCommand}, arguments: {args.Length}", target, trueCommand, args.Length);
        parser(args, connection);
    }
}
