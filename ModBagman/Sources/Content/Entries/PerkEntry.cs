﻿namespace ModBagman;

/// <summary>
/// Represents a modded perk from Arcade Mode.
/// </summary>
/// <remarks> 
/// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
/// </remarks>
[ModEntry(3500)]
public class PerkEntry : Entry<RogueLikeMode.Perks>
{
    internal PerkEntry() { }

    internal string TextEntry { get; set; }

    /// <summary>
    /// Gets or sets the action to run when a run is started. <para/>
    /// For simple perks that apply their effects at run start only,
    /// you can place their logic here. <para/>
    /// For more complex perks, you will need to check if a perk is active using <see cref="GlobalData.RogueLikePersistentData.IsPerkEquipped"/> inside a mod hook.
    /// </summary>
    public Action<PlayerView> RunStartActivator { get; set; }

    /// <summary>
    /// Gets or sets the essence cost to unlock this perk.
    /// </summary>
    public int EssenceCost { get; set; } = 15;

    /// <summary>
    /// Gets or sets the display name of this perk.
    /// </summary>
    public string Name { get; set; } = "Bishop's Shenanigans";

    /// <summary>
    /// Gets or sets the description of this perk.
    /// </summary>
    public string Description { get; set; } = "It's some weird perk or moldable!";

    /// <summary>
    /// Gets or sets the icon of the perk. The texture path is relative to "Content/".
    /// </summary>
    public string TexturePath { get; set; }

    /// <summary>
    /// Gets or sets the condition for the perk to be available in Bishop's selection.
    /// If no condition is provided, the perk is available by default.
    /// </summary>
    public Func<bool> UnlockCondition { get; set; }

    internal override void Initialize()
    {
        if (!IsVanilla)
        {
            TextEntry = $"{(int)GameID}";
        }

        Globals.Game.EXT_AddMiscText("Menus", "Perks_Name_" + TextEntry, Name);
        Globals.Game.EXT_AddMiscText("Menus", "Perks_Description_" + TextEntry, Description);

        // Texture on demand
    }

    internal override void Cleanup()
    {
        Globals.Game.EXT_RemoveMiscText("Menus", "Perks_Name_" + TextEntry);
        Globals.Game.EXT_RemoveMiscText("Menus", "Perks_Description_" + TextEntry);
        Globals.Game.Content.UnloadIfModded(TexturePath);
    }
}
