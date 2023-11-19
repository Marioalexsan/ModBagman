using Quests;
using Microsoft.Extensions.Logging;
using ModBagman.HarmonyPatches;

namespace ModBagman;

/// <summary>
/// Dummy mod that holds parsed vanilla entries.
/// </summary>
public class VanillaMod : Mod
{
    internal override bool IsBuiltin => true;

    public const string ModName = "SoG";

    internal VanillaMod()
    {

    }

    public override bool DisableObjectCreation => true;

    public override Version Version => new("0.0.0.0");

    public override string Name => ModName;

    public override void Load()
    {
        SetupAudio();

        OriginalMethods.FillTreatList(Globals.Game.xShopMenu.xTreatCurseMenu);
        ParseEntries<RogueLikeMode.TreatsCurses, CurseEntry>(this.ParseCurse);

        ParseEntries<EnemyCodex.EnemyTypes, EnemyEntry>(this.ParseEnemy);
        EnemyCodex.lxSortedCardEntries.Clear();
        EnemyCodex.lxSortedDescriptions.Clear();
        EnemyCodex.denxDescriptionDict.Clear();

        ParseEntries<EquipmentInfo.SpecialEffect, EquipmentEffectEntry>(this.ParseEquipmentEffect);

        ParseEntries<ItemCodex.ItemTypes, ItemEntry>(this.ParseItem);

        ParseEntries<Level.ZoneEnum, LevelEntry>(this.ParseLevel);

        OriginalMethods.PerkInfoInit();
        ParseEntries<RogueLikeMode.Perks, PerkEntry>(this.ParsePerk);
        RogueLikeMode.PerkInfo.lxAllPerks.Clear();

        ParseEntries<PinCodex.PinType, PinEntry>(this.ParsePin);

        ParseEntries<QuestCodex.QuestID, QuestEntry>(this.ParseQuest);

        ParseEntries<SpellCodex.SpellTypes, SpellEntry>(this.ParseSpell);

        ParseEntries<BaseStats.StatusEffectSource, StatusEffectEntry>(this.ParseStatusEffect);

        ParseEntries<Level.WorldRegion, WorldRegionEntry>(this.ParseWorldRegion);
    }

    public override void Unload()
    {
        return;
    }

    private void ParseEntries<IDType, EntryType>(Func<IDType, EntryType> parser)
        where IDType : struct, Enum
        where EntryType : Entry<IDType>
    {
        Logger.LogInformation("Parsing " + typeof(IDType) + " entries...");

        var entries = Entries.Manager<IDType, EntryType>();
        foreach (var gameID in IDExtension.GetAllSoGIDs<IDType>())
        {
            try
            {
                EntryType parsedEntry = parser.Invoke(gameID);
                entries.CreateFromExisting(parsedEntry);
            }
            catch (Exception e)
            {
                Logger.LogTrace("Failed to parse entry " + typeof(IDType).Name + ":" + gameID + ". Exception: " + e.Message);
                continue;
            }
        }
    }

    private void SetupAudio()
    {
        var wrapper = new SoundSystemWrapper(Globals.Game.xSoundSystem);

        var audio = Entries.Manager<CustomEntryID.AudioID, AudioEntry>()
            .Create(this, "", forceObjectCreation: true);

        audio.Effects.Clear();
        audio.Effects.AddRange(wrapper.SoundSplitMap.Keys);

        audio.EffectToWaveBank.Clear();
        foreach (var kvp in wrapper.SoundSplitMap)
            audio.EffectToWaveBank[kvp.Key] = kvp.Value;

        audio.Music.Clear();
        audio.Music.AddRange(wrapper.SongRegionMap.Keys);

        audio.MusicToWaveBank.Clear();
        foreach (var kvp in wrapper.SongRegionMap)
            audio.MusicToWaveBank[kvp.Key] = kvp.Value;

        audio.MusicWaveBanksToStream.Clear();
        audio.MusicWaveBanksToStream.AddRange(wrapper.StreamingMusicBanks);

        audio.EffectsSoundBank = wrapper.EffectSoundBank;
        audio.MusicSoundBank = wrapper.MusicSoundBank;

        audio.MusicWaveBank = wrapper.UniversalMusic;
        audio.EffectsWaveBank = wrapper.EffectWaveBank;

        audio.StreamedMusicWaveBank = wrapper.UniversalMusic;
        audio.StreamedEffectsWaveBank = wrapper.StreamedEffectWaveBank;
    }
}
