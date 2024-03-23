using System.Reflection.Emit;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(Game1), nameof(Game1._Chat_ParseCommand))]
static class _Chat_ParseCommand
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code, ILGenerator gen)
    {
        var codeList = code.ToList();

        Label afterRet = gen.DefineLabel();

        MethodInfo target = typeof(string).GetMethod(nameof(string.ToLowerInvariant));
        MethodInfo implementerCall = SymbolExtensions.GetMethodInfo(() => ParseModCommands(default, default, default));

        var insert = new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Ldloc_S, 2),
            new CodeInstruction(OpCodes.Ldarg_S, 1),
            new CodeInstruction(OpCodes.Ldarg_S, 2),
            new CodeInstruction(OpCodes.Call, implementerCall),
            new CodeInstruction(OpCodes.Stloc_S, 2),
            new CodeInstruction(OpCodes.Ldloc_S, 2),
            new CodeInstruction(OpCodes.Brtrue, afterRet),
            new CodeInstruction(OpCodes.Ret),
            new CodeInstruction(OpCodes.Nop).WithLabels(afterRet)
        };

        return codeList.InsertAfterMethod(target, insert);
    }

    internal static string ParseModCommands(string command, string message, long connection)
    {
        Console.WriteLine($"Calling with {command} {message}");
        string[] words = command.Split(new[] { ':' }, 2);

        if (words.Length != 2)
        {
            CAS.AddChatMessage($"[{ModBagmanMod.ModName}] Command syntax: /<mod>:<command> [args...].");
            CAS.AddChatMessage($"[{ModBagmanMod.ModName}] Use vanilla commands with /sog:<command> [args...]");
            return null;
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
            return null;
        }

        if (mod == ModManager.Vanilla)
        {
            // Intercept vanilla calls
            if (trueCommand.Equals("help", StringComparison.InvariantCultureIgnoreCase))
            {
                if (message != "")
                {
                    ParseModCommands($"{ModBagmanMod.ModName}:Help", $"{target}", connection);
                    return null;
                }
                else
                {
                    ParseModCommands($"{ModBagmanMod.ModName}:Help", $"{target}:{message}", connection);
                    return null;
                }
            }
            else
            {
                // Return the modified command
                return trueCommand;
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
            Console.WriteLine($"entry {entry != null}, parser ${parser != null}");

            if (trueCommand.Equals("help", StringComparison.InvariantCultureIgnoreCase))
            {
                if (message != "")
                {
                    ParseModCommands($"{ModBagmanMod.ModName}:Help", $"{target}", connection);
                    return null;
                }
                else
                {
                    ParseModCommands($"{ModBagmanMod.ModName}:Help", $"{target}:{message}", connection);
                    return null;
                }
            }

            CAS.AddChatMessage($"[{target}] Unknown command!");
            return null;
        }

        string[] args = message.Split(new char[] { ' ' }, options: StringSplitOptions.RemoveEmptyEntries);

        Program.Logger.LogDebug("Parsed command {target} : {trueCommand}, arguments: {args.Length}", target, trueCommand, args.Length);
        parser(args, connection);

        return null;
    }
}
