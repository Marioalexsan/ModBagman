using System.Reflection;
using Microsoft.Extensions.Logging;

namespace ModBagman;

internal static class ModLoader
{
    public static List<Mod> ObtainMods()
    {
        Program.Logger.LogInformation("Loading mods...");

        var paths = GetLoadableMods();

        var mods = LoadModsFromAssemblies(paths);

        var modOrder = BuildLoadOrder(mods);

        return modOrder;
    }

    private static List<Mod> BuildLoadOrder(List<Mod> mods)
    {
        static bool CheckDependency(Mod mod, ModDependencyAttribute dep)
        {
            if (dep.NameID != mod.Name)
            {
                return false;
            }

            if (string.IsNullOrEmpty(dep.ModVersion))
            {
                return true;
            }

            return 
                Semver.SemVersionRange.TryParseNpm(dep.ModVersion, out var range) 
                && range.Contains(Semver.SemVersion.FromVersion(mod.Version));
        }

        // Force the builtin mods to load first

        List<Mod> loadOrder = new();
        List<Mod> readyMods = null;

        Mod vanillaMod = mods.FirstOrDefault(x => x is VanillaMod);

        if (vanillaMod != null)
        {
            loadOrder.Add(vanillaMod);
            mods.Remove(vanillaMod);
        }

        Mod bagmanMod = mods.FirstOrDefault(x => x is ModBagmanMod);

        if (bagmanMod != null)
        {
            loadOrder.Add(bagmanMod);
            mods.Remove(bagmanMod);
        }

        var dependencyGraph = new Dictionary<Mod, List<Mod>>();
        var dependencies = new Dictionary<Mod, List<ModDependencyAttribute>>();

        foreach (var mod in mods)
        {
            dependencies[mod] = mod.GetType().GetCustomAttributes<ModDependencyAttribute>().ToList();
            dependencyGraph[mod] = new List<Mod>();

            Program.Logger.LogInformation($"{dependencies.Count} {dependencyGraph.Count} {mod.Name}");
        }

        while ((readyMods = dependencies.Where(x => x.Value.Count == 0).Select(x => x.Key).ToList()).Count() > 0)
        {
            Program.Logger.LogInformation($"Readymods {readyMods.Count}");
            // Sort nodes that can't be "compared" in the graph.
            // This, along with the dependency sort, ensures a deterministic order

            readyMods.Sort((x, y) => string.Compare(x.Name, y.Name));

            foreach (var mod in readyMods)
            {
                Program.Logger.LogInformation($"Load {mod}");
                dependencies.Remove(mod);
                loadOrder.Add(mod);

                foreach (var pair in dependencies)
                {
                    var depList = pair.Value;

                    for (int i = 0; i < depList.Count; i++)
                    {
                        if (CheckDependency(mod, depList[i]))
                        {
                            dependencyGraph[mod].Add(pair.Key);
                            depList.RemoveAt(i);
                            i -= 1;
                        }
                    }
                }
            }
        }

        // Any leftover mods in dependency graph will fail to load due to missing dependencies

        foreach (var pair in dependencies)
        {
            Program.Logger.LogError("Mod {mod} cannot be loaded. It is missing the following mod dependencies:", pair.Key.Name);

            foreach (var dep in pair.Value)
            {
                string id = $"    {dep.NameID}";
                string version = string.IsNullOrEmpty(dep.ModVersion) ? "" : ($", Version {dep.ModVersion}");

                Program.Logger.LogError("{id}{version}", id, version);
            }
        }

        return loadOrder;
    }

    private static List<string> GetLoadableMods()
    {
        // TODO: Rewrite ignored mods source
        var ignoredMods = Program.Config.IgnoredMods;

        var modFolders = Enumerable.Empty<string>().Append(Globals.ModFolderPath).Concat(Program.Config.ExtraModFolders);
        
        int totalCount = 0;
        int selectedCount = 0;

        List<string> selectedMods = new();

        foreach (var modFolder in modFolders)
        {
            var candidates = Directory.GetFiles(modFolder)
                .Where(x => x.EndsWith(".dll") || x.EndsWith(".zip"))
                .Concat(Directory.GetDirectories(modFolder).Where(x => x.EndsWith("_csxdev")))
                .ToList();

            var selected = candidates
                .Where(x => !ignoredMods.Contains(Path.GetFileName(x.TrimEnd('/', '\\'))))
                .ToList();

            totalCount += candidates.Count;
            selectedCount += selected.Count;

            selectedMods.AddRange(selected);
        }

        Program.Logger.LogInformation($"Found {totalCount - selectedCount} mods that are present in the ignore list.");
        Program.Logger.LogInformation($"Selecting {selectedCount} other mods for loading.");
        Program.Logger.LogInformation($"Also will attempt loading {Program.Config.ExtraModPaths.Count} mods from extra mod paths.");

        selectedMods.AddRange(Program.Config.ExtraModPaths);

        return selectedMods;
    }

    private static Mod LoadMod(string path)
    {
        if (path.EndsWith(".dll"))
            return LoadCSharpMod(path);

        return null;
    }

    private static List<Mod> LoadModsFromAssemblies(IEnumerable<string> paths)
    {
        var mods = new List<Mod>()
        {
            new VanillaMod(),
            new ModBagmanMod()
        };

        var usedNames = new List<string>();

        mods.AddRange(paths.Select(path =>
        {
            Mod mod = LoadMod(path);

            if (mod == null)
                return null;

            bool conflictingID = usedNames.Any(x => x == mod.Name);

            if (conflictingID)
            {
                string shortPath = ShortenModPaths(path);

                Program.Logger.LogError("Mod {shortPath} with NameID {mod.NameID} conflicts with a previously loaded mod.", shortPath, mod.Name);
                return null;
            }

            mod.LoadedFrom = path;
            usedNames.Add(mod.Name);
            Program.Logger.LogInformation($"Candidate mod for loading: {mod.Name}");
            return mod;
        }).Where(mod => mod != null));

        return mods;
    }

    /// <summary>
    /// Finds and replaces commonly occuring game paths with shortened forms.
    /// </summary>
    private static string ShortenModPaths(string path)
    {
        return path
            .Replace('/', '\\')
            .Replace(Directory.GetCurrentDirectory() + @"\Content\ModContent", "(ModContent)")
            .Replace(Directory.GetCurrentDirectory() + @"\Content\Mods", "(Mods)")
            .Replace(Directory.GetCurrentDirectory() + @"\Content", "(Content)")
            .Replace(Directory.GetCurrentDirectory(), "(SoG)");
    }

    private static Mod LoadCSharpMod(string assemblyPath)
    {
        string shortPath = ShortenModPaths(assemblyPath);

        Program.Logger.LogInformation("Loading C# mod from {path}.", shortPath);

        try
        {
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            Type type = assembly.DefinedTypes.First(t => t.BaseType == typeof(Mod));
            Mod mod = Activator.CreateInstance(type, true) as Mod;

            mod.ValidateAndSetup();

            return mod;
        }
        catch (MissingMethodException)
        {
            Program.Logger.LogError("Failed to load mod {modPath}. No parameterless constructor found!", shortPath);
            Program.Logger.LogError("You need to define a parameterless constructor so that the mod tool can instantiate the mod.");
        }
        catch (BadImageFormatException) { /* Ignore non-managed DLLs */ }
        catch (Exception e)
        {
            Program.Logger.LogError("Failed to load mod {modPath}. An unknown exception occurred: {e}", shortPath, ShortenModPaths(e.ToString()));
        }

        return null;
    }
}
