﻿using Quests;
using System.Reflection;

namespace ModBagman;

/// <summary>
/// Represents a modded game object.
/// </summary>
public abstract class Entry<IDType> where IDType : struct, Enum
{
    internal Entry() { }

    /// <summary>
    /// Gets the mod that created this entry.
    /// </summary>
    public Mod Mod { get; internal set; }

    /// <summary>
    /// Gets the mod ID of this entry.
    /// </summary>
    public string ModID { get; internal set; }

    /// <summary>
    /// Gets the vanilla ID of this entry.
    /// </summary>
    public IDType GameID { get; internal set; }

    /// <summary>
    /// Is this a vanilla entry?
    /// (this is the same as it being part of <see cref="VanillaMod"/>).
    /// </summary>
    public bool IsVanilla => Mod.GetType() == typeof(VanillaMod);

    /// <summary>
    /// Is this a modded entry?
    /// </summary>
    public bool IsModded => !IsVanilla && Mod != null;

    /// <summary>
    /// Is this an unknown entry?
    /// (it's either an invalid item or an item from an unloaded mod)
    /// </summary>
    public bool IsUnknown => !(IsVanilla || IsModded);

    private bool Ready { get; set; } = false;

    internal void InitializeEntry()
    {
        if (!Ready)
        {
            Initialize();
            Ready = true;
        }
    }

    internal void CleanupEntry()
    {
        if (Ready)
        {
            Cleanup();
            Ready = false;
        }
    }

    internal abstract void Initialize();

    internal abstract void Cleanup();
}

internal static class Entries
{
    private static void CheckInit()
    {
        if (_managers.Count > 0)
            return;

        Type[] _managerConstructorTypes = new[]
        {
            typeof(long),
            typeof(long)
        };

        // Create managers for all registered entries in the assembly
        var entryTypes = typeof(ModManager).Assembly.DefinedTypes
            .Select(x => (x, x.GetCustomAttribute<ModEntryAttribute>()))
            .Where(x => x.Item2 != null);

        foreach ((var entryType, var attr) in entryTypes)
        {
            var type = typeof(EntryManager<,>).MakeGenericType(entryType.BaseType.GenericTypeArguments[0], entryType);

            _managers[type] = (IEntryManager)type.GetConstructor(_managerConstructorTypes).Invoke(new object[] { attr.Start, attr.Count });
        }
    }

    private static readonly Dictionary<Type, IEntryManager> _managers = new();

    internal static EntryManager<IDType, EntryType> Manager<IDType, EntryType>()
        where IDType : struct, Enum
        where EntryType : Entry<IDType>
    {
        CheckInit();
        return _managers[typeof(EntryManager<IDType, EntryType>)] as EntryManager<IDType, EntryType>;
    }

    internal static IEnumerable<IEntryManager> Managers()
    {
        CheckInit();
        return _managers.Values;
    }

    public static EntryManager<CustomEntryID.AudioID, AudioEntry> Audio
        => Manager<CustomEntryID.AudioID, AudioEntry>();

    public static EntryManager<CustomEntryID.CommandID, CommandEntry> Commands
        => Manager<CustomEntryID.CommandID, CommandEntry>();

    public static EntryManager<RogueLikeMode.TreatsCurses, CurseEntry> Curses
        => Manager<RogueLikeMode.TreatsCurses, CurseEntry>();

    public static EntryManager<EnemyCodex.EnemyTypes, EnemyEntry> Enemies
        => Manager<EnemyCodex.EnemyTypes, EnemyEntry>();

    public static EntryManager<EquipmentInfo.SpecialEffect, EquipmentEffectEntry> EquipmentEffects
        => Manager<EquipmentInfo.SpecialEffect, EquipmentEffectEntry>();

    public static EntryManager<ItemCodex.ItemTypes, ItemEntry> Items
        => Manager<ItemCodex.ItemTypes, ItemEntry>();

    public static EntryManager<Level.ZoneEnum, LevelEntry> Levels
        => Manager<Level.ZoneEnum, LevelEntry>();

    public static EntryManager<CustomEntryID.NetworkID, NetworkEntry> Network
        => Manager<CustomEntryID.NetworkID, NetworkEntry>();

    public static EntryManager<RogueLikeMode.Perks, PerkEntry> Perks
        => Manager<RogueLikeMode.Perks, PerkEntry>();

    public static EntryManager<PinCodex.PinType, PinEntry> Pins
        => Manager<PinCodex.PinType, PinEntry>();

    public static EntryManager<QuestCodex.QuestID, QuestEntry> Quests
        => Manager<QuestCodex.QuestID, QuestEntry>();

    public static EntryManager<CustomEntryID.SaveID, SaveEntry> Saves
        => Manager<CustomEntryID.SaveID, SaveEntry>();

    public static EntryManager<SpellCodex.SpellTypes, SpellEntry> Spells
        => Manager<SpellCodex.SpellTypes, SpellEntry>();

    public static EntryManager<BaseStats.StatusEffectSource, StatusEffectEntry> StatusEffects
        => Manager<BaseStats.StatusEffectSource, StatusEffectEntry>();

    public static EntryManager<Level.WorldRegion, WorldRegionEntry> WorldRegions
        => Manager<Level.WorldRegion, WorldRegionEntry>();
}
