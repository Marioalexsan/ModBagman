﻿namespace ModBagman;

/// <summary>
/// Delegate that initializes the blueprint of a level.
/// </summary>
/// <param name="blueprint"> The level blueprint to initialize. </param>
public delegate void LevelBuilder(LevelBlueprint blueprint);

/// <summary>
/// Delegate that loads a level, after objects in the level blueprint have been created.
/// </summary>
/// <param name="staticOnly"> Whenever to limit instantiation to static (non-networked) objects only. </param>
public delegate void LevelLoader(bool staticOnly);

/// <summary>
/// Represents a modded level, and defines ways to create it.
/// </summary>
/// <remarks> 
/// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
/// </remarks>
[ModEntry(5600)]
public class LevelEntry : Entry<Level.ZoneEnum>
{
    internal LevelEntry() { }

    /// <summary>
    /// Gets or sets the builder for this level.
    /// The builder is used to initialize the level blueprint with all of the static objects that will
    /// appear in it (spawn points, static environments, etc.)
    /// </summary>
    public LevelBuilder Builder { get; set; }

    /// <summary>
    /// Gets or sets the level loader for this level. <para/>
    /// The loader is called whenever this level is entered. Its task is to initialize all of the 
    /// dynamic objects that will appear in the level (NPCs, state dependent stuff, etc.)
    /// </summary>
    public LevelLoader Loader { get; set; }

    /// <summary>
    /// Gets or sets the world region of this level. <para/>
    /// The world region is used for some game logic, such as audio loading and unloading.
    /// </summary>
    public Level.WorldRegion WorldRegion { get; set; }

    internal override void Initialize()
    {
        // Nothing for now
    }

    internal override void Cleanup()
    {
        // Nothing for now
    }
}
