﻿using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Extensions.Logging;

namespace ModBagman;

/// <summary>
/// Provides a common access point to default objects.
/// </summary>
public static class Globals
{
    private static readonly FieldInfo s_sVersion = AccessTools.Field(typeof(Game1), "sVersion");
    private static readonly FieldInfo s_spriteBatch = AccessTools.Field(typeof(Game1), "spriteBatch");

    /// <summary>
    /// The version of the mod tool.
    /// </summary>
    public static Version ModBagmanVersion => new(0, 1, 0);

    /// <summary>
    /// Secrets of Grindea's game instance.
    /// </summary>
    public static DeveloperConsole Console { get; private set; }

    /// <summary>
    /// Secrets of Grindea's game instance.
    /// </summary>
    public static Game1 Game { get; private set; }

    public static string ModData => Path.Combine(Directory.GetCurrentDirectory(), "ModBagmanData");

    public static string AppDataPath => Path.Combine(ModData, "AppData");
    public static string ModContentPath = Path.Combine(ModData, "ModContent");
    public static string ModFolderPath = Path.Combine(ModData, "Mods");
    public static string LogPath => Path.Combine(ModData, "Logs");

    /// <summary>
    /// The game's initial (vanilla) version.
    /// </summary>
    public static string GrindeaVersion { get; private set; }

    /// <summary>
    /// The game's modded version, long form.
    /// </summary>
    public static string GameVersionFull => s_sVersion.GetValue(Game) as string;

    /// <summary>
    /// The game's sprite batch. 
    /// </summary>
    public static SpriteBatch SpriteBatch => s_spriteBatch.GetValue(Game) as SpriteBatch;

    internal static void InitializeGlobals()
    {
        Game = (Game1)typeof(Game1).Assembly.GetType("SoG.Program").GetField("game").GetValue(null);
        Game.sAppData = AppDataPath.TrimEnd('/') + "/";
        Game.xGameSessionData.xRogueLikeSession.bTemporaryHighScoreBlock = true;
        Console = new();
    }

    internal static void UpdateVersionNumber()
    {
        GrindeaVersion = Game.sVersionNumberOnly;

        SetVersionTypeAsModded(true);

        AccessTools.Field(typeof(Game1), "sVersion").SetValue(Game, GrindeaVersion + " with ModBagman " + ModBagmanVersion);

        Program.Logger.LogInformation("Game Long Version: {Version}", GameVersionFull);
        Program.Logger.LogInformation("Game Vanilla Version: {Version}", GrindeaVersion);
    }

    /// <summary>
    /// Changes the perceived version of the game.
    /// This is used when saving / loading the vanilla saves, so that they don't write the modded version.
    /// </summary>
    internal static void SetVersionTypeAsModded(bool modded)
    {
        Game.sVersionNumberOnly = GrindeaVersion + (modded ? "-modded" : "");
    }
}
