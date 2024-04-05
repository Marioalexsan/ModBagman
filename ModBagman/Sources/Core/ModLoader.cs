using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO.Compression;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

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

    private static List<Mod> BuildLoadOrder(IEnumerable<Mod> mods)
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

        var dependencyGraph = new Dictionary<Mod, List<Mod>>();
        var dependencies = new Dictionary<Mod, List<ModDependencyAttribute>>();

        foreach (var mod in mods)
        {
            dependencies[mod] = mod is JavaScriptMod jsMod
                ? jsMod.ListedDependencies.Select(x => new ModDependencyAttribute(x.Key, x.Value)).ToList()
                : mod.GetType().GetCustomAttributes<ModDependencyAttribute>().ToList();
            dependencyGraph[mod] = new List<Mod>();

            Program.Logger.LogInformation($"{dependencies.Count} {dependencyGraph.Count} {mod.Name}");
        }

        List<Mod> loadOrder = new();
        List<Mod> readyMods = null;

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
        var ignoredMods = Program.ReadConfig()?.GetSection("IgnoredMods")?.Get<List<string>>() ?? Enumerable.Empty<string>();

        var modFolder = Path.Combine(Directory.GetCurrentDirectory(), "Mods");

        List<string> fullPathIgnored = ignoredMods.Select(x => Path.Combine(modFolder, x)).ToList();

        var candidates = Directory.GetFiles(modFolder)
            .Where(x => x.EndsWith(".dll") || x.EndsWith(".zip"))
            .Concat(Directory.GetDirectories(modFolder).Where(x => x.EndsWith("_jsdev")))
            .Concat(Directory.GetDirectories(modFolder).Where(x => x.EndsWith("_csxdev")))
            .ToList();

        var selected = candidates
            .Where(x => !fullPathIgnored.Contains(x))
            .ToList();

        int totalCount = candidates.Count;
        int selectedCount = selected.Count;
        int ignoreCount = totalCount - selectedCount;

        Program.Logger.LogInformation("Found {ignoreCount} mods that are present in the ignore list.", ignoreCount);
        Program.Logger.LogInformation("Selecting {selectedCount} other mods for loading.", selectedCount);

        return selected;
    }

    private static Mod LoadMod(string path)
    {
        if (path.EndsWith(".dll"))
            return LoadCSharpMod(path);

        if (path.EndsWith(".js.zip"))
            return LoadJavaScriptArchive(path);

        if (path.EndsWith(".csx.zip"))
            return LoadCSharpScriptArchive(path);

        if (Directory.Exists(path))
        {
            if (path.EndsWith("_jsdev"))
                return LoadJavaScriptFolder(path);

            if (path.EndsWith("_csxdev"))
                return LoadCSharpScriptFolder(path);
        }

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

    private static Mod LoadJavaScriptFolder(string folderPath)
    {
        string shortPath = ShortenModPaths(folderPath);

        Program.Logger.LogInformation("Loading JavaScript mod from {path}.", shortPath);

        try
        {
            Queue<string> paths = new();

            paths.Enqueue(folderPath);

            Dictionary<string, string> sources = new();

            var split = new[]
            {
                '/'
            };

            while (paths.Count > 0)
            {
                var folder = paths.Dequeue();
                Program.Logger.LogInformation($"Folder {folder}");

                foreach (var path in Directory.GetDirectories(folder))
                    paths.Enqueue(path);

                foreach (var file in Directory.GetFiles(folder).Where(x => x.EndsWith(".js")))
                {
                    using StreamReader reader = new StreamReader(File.OpenRead(file));
                    sources[file] = reader.ReadToEnd();
                    Program.Logger.LogInformation($"Script .js {file}");
                }
            }

            Program.Logger.LogInformation("Loaded modules: ");

            foreach (var item in sources)
            {
                Program.Logger.LogInformation(item.Key);
            }

            if (!sources.Any(x => x.Key == "index.js"))
            {
                Program.Logger.LogError("Failed to load development mod. No index.js entry point found.", shortPath);
                return null;
            }

            var mod = new JavaScriptMod(sources);

            mod.ValidateAndSetup();

            return mod;
        }
        catch (Exception e)
        {
            Program.Logger.LogError("Failed to load mod {modPath}. An unknown exception occurred: {e}", shortPath, ShortenModPaths(e.ToString()));
        }

        return null;
    }

    private static Mod LoadJavaScriptArchive(string zipPath)
    {
        string shortPath = ShortenModPaths(zipPath);

        Program.Logger.LogInformation("Loading JavaScript mod from {path}.", shortPath);

        try
        {
            using ZipArchive archive = ZipFile.OpenRead(zipPath);

            var split = new[]
            {
                '/'
            };

            var pathParts = archive.Entries.First().FullName.Split(split, 2);
            var isStacked = pathParts.Length == 2 && pathParts[0] == Path.GetFileNameWithoutExtension(zipPath);

            if (!archive.Entries.Any(x => (isStacked ? x.FullName.Split(split, 2)[1] : x.FullName) == "index.js"))
            {
                Program.Logger.LogError("Failed to load mod. No index.js entry point found.", shortPath);
                return null;
            }

            Dictionary<string, string> sources = new();

            foreach (var entry in archive.Entries.Where(x => x.Name.EndsWith(".js")))
            {
                using StreamReader reader = new StreamReader(entry.Open());
                sources[isStacked ? entry.FullName.Split(split, 2)[1] : entry.FullName] = reader.ReadToEnd();
            }

            var mod = new JavaScriptMod(sources);

            mod.ValidateAndSetup();

            return mod;
        }
        catch (Exception e)
        {
            Program.Logger.LogError("Failed to load mod {modPath}. An unknown exception occurred: {e}", shortPath, ShortenModPaths(e.ToString()));
        }

        return null;
    }

    private static Mod LoadCSharpScriptFolder(string folderPath)
    {
        string shortPath = ShortenModPaths(folderPath);

        Program.Logger.LogInformation("Loading CSX mod from {path}.", shortPath);

        try
        {
            Queue<string> paths = new();

            paths.Enqueue(folderPath);

            Dictionary<string, string> sources = new();

            var split = new[]
            {
                '/'
            };

            while (paths.Count > 0)
            {
                var folder = paths.Dequeue();
                Program.Logger.LogInformation($"Folder {folder}");

                foreach (var path in Directory.GetDirectories(folder))
                    paths.Enqueue(path);

                foreach (var file in Directory.GetFiles(folder).Where(x => x.EndsWith(".csx") || x.EndsWith(".cs")))
                {
                    using StreamReader reader = new StreamReader(File.OpenRead(file));
                    sources[file] = reader.ReadToEnd();
                    Program.Logger.LogInformation($"Script .csx {file}");
                }
            }

            Program.Logger.LogInformation("Loaded modules: ");

            foreach (var item in sources)
            {
                Program.Logger.LogInformation(item.Key);
            }

            var mod = CompileCSharpScriptAssembly(sources);

            mod.ValidateAndSetup();

            return mod;
        }
        catch (Exception e)
        {
            Program.Logger.LogError("Failed to load mod {modPath}. An unknown exception occurred: {e}", shortPath, ShortenModPaths(e.ToString()));
        }

        return null;
    }

    private static Mod LoadCSharpScriptArchive(string zipPath)
    {
        string shortPath = ShortenModPaths(zipPath);

        Program.Logger.LogInformation("Loading CSX mod from {path}.", shortPath);

        try
        {
            using ZipArchive archive = ZipFile.OpenRead(zipPath);

            var split = new[]
            {
                '/'
            };

            var pathParts = archive.Entries.First().FullName.Split(split, 2);
            var isStacked = pathParts.Length == 2 && pathParts[0] == Path.GetFileNameWithoutExtension(zipPath);

            Dictionary<string, string> sources = new();

            foreach (var entry in archive.Entries.Where(x => x.Name.EndsWith(".csx") || x.Name.EndsWith(".cs")))
            {
                using StreamReader reader = new StreamReader(entry.Open());
                sources[isStacked ? entry.FullName.Split(split, 2)[1] : entry.FullName] = reader.ReadToEnd();
            }

            var mod = CompileCSharpScriptAssembly(sources);

            mod.ValidateAndSetup();

            return mod;
        }
        catch (Exception e)
        {
            Program.Logger.LogError("Failed to load mod {modPath}. An unknown exception occurred: {e}", shortPath, ShortenModPaths(e.ToString()));
        }

        return null;
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

    private static readonly Dictionary<string, MetadataReference> References;

    private static void AddAssembly(string assemblyDll)
    {
        var file = Path.GetFullPath(assemblyDll);

        if (!File.Exists(file))
        {
            file = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), assemblyDll);

            if (!File.Exists(file))
            {
                Program.Logger.LogWarning($"Couldn't find assembly {assemblyDll}: {file}");
                return;
            }
        }

        if (References.Any(x => x.Key == file))
            return;

        Program.Logger.LogInformation($"Loading assembly {assemblyDll}: {file}");
        var reference = MetadataReference.CreateFromFile(file);

        References[file] = reference;
    }

    private static void AddAssembly(Type type)
    {
        AddAssembly(type.Assembly.Location);
    }

    static ModLoader()
    {
        References = new();

        AddAssembly(typeof(Mod));
        AddAssembly(typeof(Game1));
        AddAssembly("mscorlib.dll");
        AddAssembly("System.dll");
        AddAssembly("System.Core.dll");
        AddAssembly("Microsoft.CSharp.dll");
        AddAssembly("System.Net.Http.dll");
        AddAssembly("System.IO.Compression");
        AddAssembly("System.IO.Compression.FileSystem.dll");
        AddAssembly(typeof(Vector2));
        AddAssembly(typeof(Game));
        AddAssembly(typeof(SpriteBatch));
        AddAssembly(typeof(SoundSystem));
        AddAssembly(typeof(ZipArchive));
    }

    private static Mod CompileCSharpScriptAssembly(Dictionary<string, string> sources)
    {
        var compilationOptions = new CSharpCompilationOptions(
            Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: Microsoft.CodeAnalysis.OptimizationLevel.Debug
            );

        var syntaxTrees = sources.Select(x => SyntaxFactory.ParseSyntaxTree(x.Value));

        var compilation = CSharpCompilation.Create($"ModAssembly-{Guid.NewGuid()}")
            .WithOptions(compilationOptions)
            .WithReferences(References.Values.ToArray())
            .AddSyntaxTrees(syntaxTrees);

        using var codeStream = new MemoryStream();

        var compilationResult = compilation.Emit(codeStream);

        if (!compilationResult.Success)
        {
            int maxErrors = 10;

            var sb = new StringBuilder();
            foreach (var diag in compilationResult.Diagnostics.Take(maxErrors))
            {
                sb.AppendLine(diag.ToString());
            }

            throw new InvalidOperationException("CSharp script mod is invalid\n" + sb.ToString());
        }

        var assembly = Assembly.Load(codeStream.ToArray());
        Type type = assembly.DefinedTypes.First(t => t.BaseType == typeof(Mod));
        Mod mod = Activator.CreateInstance(type, true) as Mod;
        mod.CompiledFromCSharp = true;

        return mod;
    }
}
