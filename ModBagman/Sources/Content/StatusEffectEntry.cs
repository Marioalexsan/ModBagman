using Microsoft.Xna.Framework.Graphics;

namespace ModBagman;

/// <summary>
/// Represents a modded status effect.
/// </summary>
/// <remarks> 
/// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
/// </remarks>
[ModEntry(1000)]
public class StatusEffectEntry : Entry<BaseStats.StatusEffectSource>
{
    internal StatusEffectEntry() { }

    /// <summary>
    /// Gets or sets the icon's texture path.
    /// A null or empty string will load NullTex instead.
    /// If specified, <see cref="TextureLoader"/> takes priority over <see cref="TexturePath"/>.
    /// </summary>
    public string TexturePath { get; set; }

    /// <summary>
    /// Gets or sets the icon's texture loader.
    /// A null texture will be replaced by NullTex instead.
    /// If specified, <see cref="TextureLoader"/> has a higher priority.
    /// </summary>
    public Func<Texture2D> TextureLoader { get; set; }

    internal override void Initialize()
    {
        // Nothing, texture is loaded on demand
    }

    internal override void Cleanup()
    {
        Globals.Game.Content.UnloadIfModded(TexturePath);
    }
}
