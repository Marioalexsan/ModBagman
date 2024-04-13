using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using CommandLine;
using CommandLine.Text;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ModBagman;

internal static class Program
{
    private static string GetPatchLoadingText()
    {
        var random = new Random();

        string defaultText = "Patching Game...";

        if (random.NextDouble() < 0.8f)
            return defaultText;

        var easterEggs = new Dictionary<string, int>
        {
            ["Propagating Bagmen..."] = 10,
            ["Operating the Whachamacallit..."] = 5,
            ["Adding human card..."] = 10,
            ["Loading TeddyCode(tm)..."] = 5,
            ["Rigging card drop chances..."] = 5,
            ["Adding lootboxes..."] = 5,
            ["Modding the mod so that you can mod while modding..."] = 5
        };

        var weightRolled = random.Next(easterEggs.Values.Sum());
        var chosenText = "";

        foreach (var kvp in easterEggs)
        {
            chosenText = kvp.Key;
            weightRolled -= kvp.Value;

            if (weightRolled <= 0)
                break;
        }

        return chosenText;
    }

    public static Harmony HarmonyInstance { get; } = new Harmony("ModBagman");

    public static DateTime LaunchTime { get; private set; }

    public static ILoggerFactory CreateLogFactory(bool multiLine) => LoggerFactory.Create(config =>
    {
        config.AddFile(Path.Combine(Globals.AppDataPath, "Logs", "EventLog-{Date}.txt"), 
            outputTemplate: "{Timestamp:o} {RequestId,13} [{Level:u3}] [{SourceContext:1}] {Message} {NewLine}{Exception}");
    });

    private static IConfigurationBuilder _configBuilder;
    private static string GetConfigPath() => Path.Combine(Globals.AppDataPath, "ModBagmanConfig.json");

    private static IConfiguration _configuration = null;
    public static IConfiguration ReadConfig()
    {
        if (_configuration != null)
            return _configuration;

        if (!File.Exists(GetConfigPath()))
        {
            const string BaseConfig = """
                {
                    "IgnoredMods": [
                        
                    ],
                    "HarmonyDebug": false,
                    "PrintAutoSplitOffsets": false
                }
                """;

            File.WriteAllText(GetConfigPath(), BaseConfig);
            Thread.Sleep(10);
        }

        try
        {
            return _configuration = _configBuilder.Build();
        }
        catch
        {
            Logger.LogError("Failed to read configuration file! Please check if ModBagmanConfig.json is valid.");
            return null;
        }
    }

    public static ILogger Logger { get; } = CreateLogFactory(false).CreateLogger("ModBagman");

    public static ILogger ExceptionLogger { get; } = CreateLogFactory(true).CreateLogger("ModBagman");

    internal static bool HasCrashed { get; set; } = false;

    public static void Main(string[] args)
    {
        LaunchTime = DateTime.Now;

        var options = ParseArguments(args);

        if (options.Version)
        {
            Logger.LogInformation($"ModBagman version {Globals.ModBagmanVersion}.");
            return;
        }

        try
        {
            CheckFirstTimeBoot();
            HarmonyMetaPatch();
            SetupModBagman();
            InvokeSoGMain(args);
        }
        catch (Exception e)
        {
            ErrorHelper.ForceExit(e, skipLogging: true);
        }

        if (HasCrashed)
        {
            Logger.LogCritical("Exiting game due to crash.");

            MessageBox.Show("Game crashed due to an exception!\nPlease check the logs in %appdata%/ModBagman/Logs.", "GAME DEADED", MessageBoxButton.OK);
        }
        else
        {
            Logger.LogInformation("Game closed normally.");
        }
    }

    private static void CheckFirstTimeBoot()
    {
        if (!Directory.Exists(Globals.AppDataPath))
        {
            var result = MessageBox.Show(AsciiArtResources.CopySavesNotice, "Copy saves?", MessageBoxButton.YesNo);

            Directory.CreateDirectory(Globals.AppDataPath);
            Directory.CreateDirectory(Path.Combine(Globals.AppDataPath, "Characters"));
            Directory.CreateDirectory(Path.Combine(Globals.AppDataPath, "Worlds"));

            if (result == MessageBoxResult.Yes)
            {
                var vanilla = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Secrets of Grindea");

                for (int i = 0; i < 9; i++)
                {
                    if (File.Exists(Path.Combine(vanilla, "Characters", i + ".cha")))
                        File.Copy(Path.Combine(vanilla, "Characters", i + ".cha"), Path.Combine(Globals.AppDataPath, "Characters", i + ".cha"), true);

                    if (File.Exists(Path.Combine(vanilla, "Worlds", i + ".wld")))
                        File.Copy(Path.Combine(vanilla, "Worlds", i + ".wld"), Path.Combine(Globals.AppDataPath, "Worlds", i + ".wld"), true);
                }

                if (File.Exists(Path.Combine(vanilla, "arcademode.sav")))
                    File.Copy(Path.Combine(vanilla, "arcademode.sav"), Path.Combine(Globals.AppDataPath, "arcademode.sav"), true);

                MessageBox.Show(AsciiArtResources.SavesCopiedSuccessfullyNotice, "Saves copied successfully", MessageBoxButton.OK);
            }
        }
    }

    private static void SetupModBagman()
    {
        // Should force Secrets Of Grindea.exe assembly to be loaded
        _ = typeof(Game1);

        Directory.CreateDirectory("Mods");
        Directory.CreateDirectory("ModContent");
        Directory.CreateDirectory(Path.Combine(Globals.AppDataPath, "Logs"));
        _configBuilder = new ConfigurationBuilder().AddJsonFile(GetConfigPath());

        // Do patching in two stages

        HarmonyInstance.Patch(
            AccessTools.Method(typeof(Game1), "Initialize"),
            prefix: new HarmonyMethod(typeof(Program), nameof(ModBagmanInitializeHook))
        );

        HarmonyInstance.Patch(
            AccessTools.Method(typeof(Game1), "Update"),
            prefix: new HarmonyMethod(typeof(Program), nameof(ModBagmanUpdateHook))
        );

        HarmonyInstance.Patch(
            AccessTools.Method(typeof(Game1), "Draw"),
            prefix: new HarmonyMethod(typeof(Program), nameof(ModBagmanDrawHook))
        );

        HarmonyInstance.CreateReversePatcher(
            AccessTools.Method(typeof(Game), "Update"),
            AccessTools.Method(typeof(Program), nameof(GameUpdateBase))
        ).Patch();

        HarmonyInstance.CreateReversePatcher(
            AccessTools.Method(typeof(Game), "Draw"),
            AccessTools.Method(typeof(Program), nameof(GameDrawBase))
        ).Patch();
    }

    private static void InvokeSoGMain(string[] args)
    {
        Logger.LogInformation("Starting game...");
        typeof(Game1).Assembly
            .GetType("SoG.Program")
            .GetMethod("Main", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            .Invoke(null, new object[] { args });
    }

    private static bool GamePatchingStarted = false;
    private static bool GamePatched = false;
    private static string CurrentMethod = "";
    private static readonly Stopwatch CurrentPatchingTime = new();

    private static void GameUpdateBase(Game __instance, GameTime gameTime)
    {
        throw new InvalidOperationException("Stub method.");
    }

    private static void GameDrawBase(Game __instance, GameTime gameTime)
    {
        throw new InvalidOperationException("Stub method.");
    }

    private static void ModBagmanInitializeHook()
    {
        Globals.InitializeGlobals();
    }

    private static bool ModBagmanUpdateHook(Game1 __instance, GameTime gameTime)
    {
        if (GamePatched)
            return true;

        if (!GamePatchingStarted)
        {
            GamePatchingStarted = true;
            CurrentPatchingTime.Start();
            new Thread(ApplyGamePatches).Start();
        }

        GameUpdateBase(__instance, gameTime);
        return false;
    }

    private static readonly string PatchLoadingText = GetPatchLoadingText();

    private static bool ModBagmanDrawHook(Game1 __instance, GameTime gameTime)
    {
        if (GamePatched)
            return true;

        var font = FontManager.GetFont(FontManager.FontType.Reg7);

        string text = PatchLoadingText;
        string status = $"{(!string.IsNullOrEmpty(CurrentMethod) ? $"Patching {CurrentMethod}" : "Running Harmony logic")} ({CurrentPatchingTime.Elapsed.TotalSeconds:F1}s)";
        Texture2D bag = Globals.Game.Content.Load<Texture2D>("GUI/Dialogue/Portraits/Bag/default");

        Globals.Game.GraphicsDevice.Clear(Color.Black);

        Globals.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null, null);

        // The one and only Bag
        Globals.SpriteBatch.Draw(bag, new Vector2(640, bag.Height + 100) / 2, null, Color.White, (float)(gameTime.TotalGameTime.TotalSeconds * Math.PI * 2 / 3), new Vector2(bag.Width / 2, bag.Height * 2 / 3), 1f, SpriteEffects.None, 0f);

        // Patching game text
        Globals.SpriteBatch.DrawString(font, text, new Vector2(640, 360) / 2, Color.White, 0f, font.MeasureString(text) / 2, 2f, SpriteEffects.None, 0f);
        
        // Current method and total time status
        Globals.SpriteBatch.DrawString(font, status, new Vector2(640, 360) / 2 + new Vector2(0, font.MeasureString(text).Y + 10), Color.White, 0f, font.MeasureString(status) / 2, 2f, SpriteEffects.None, 0f);
        
        Globals.SpriteBatch.End();

        GameDrawBase(__instance, gameTime);
        return false;
    }

    private static void ApplyGamePatches()
    {
        Logger.LogInformation("Applying Patches...");

        var lastDebugMode = Harmony.DEBUG;
        Harmony.DEBUG = ReadConfig().GetValue("HarmonyDebug", false);

        if (Harmony.DEBUG)
            Logger.LogInformation("Using Harmony Debug mode.");

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        HarmonyInstance.PatchAll(typeof(ModManager).Assembly);
        stopwatch.Stop();

        Harmony.DEBUG = lastDebugMode;

        Logger.LogInformation("Patched {Count} methods in {Time:F2} seconds!", HarmonyInstance.GetPatchedMethods().Count(), stopwatch.Elapsed.TotalSeconds);

        GamePatched = true;
        CurrentPatchingTime.Stop();
    }

    private static void HarmonyMetaPatch()
    {
        HarmonyInstance.Patch(
            AccessTools.Method("HarmonyLib.PatchFunctions:UpdateWrapper"),
            prefix: new HarmonyMethod(typeof(Program), nameof(PrefixStopwatchStart)),
            postfix: new HarmonyMethod(typeof(Program), nameof(PostfixStopwatchStop))
        );
    }

    private static void PrefixStopwatchStart(out Stopwatch __state, MethodBase original)
    {
        CurrentMethod = original.Name;
        __state = new Stopwatch();
        __state.Start();
    }

    private static void PostfixStopwatchStop(Stopwatch __state, MethodBase original)
    {
        if (__state != null)
        {
            __state.Stop();

            if (__state.Elapsed > TimeSpan.FromSeconds(0.25))
                Logger.LogWarning($"Patch is taking a long time! ({__state.Elapsed.TotalSeconds:F2}s) ({original.Name})");
        }
        CurrentMethod = "";
    }

    private static CLIOptions ParseArguments(string[] args)
    {
        var parser = new Parser(config =>
        {
            config.HelpWriter = Console.Out;
            config.AutoVersion = false;
        });

        var parserResult = parser.ParseArguments<CLIOptions>(args);
           
        parserResult.WithNotParsed((e) =>
            {
                Environment.Exit(1);
            });

        return parserResult.Value;
    }
}
