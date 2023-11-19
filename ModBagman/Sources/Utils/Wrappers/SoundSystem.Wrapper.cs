using Microsoft.Xna.Framework.Audio;
using System.Reflection;

namespace ModBagman;

internal class SoundSystemWrapper
{
    static readonly FieldInfo s_musicWaveBank = AccessTools.Field(typeof(SoundSystem), "musicWaveBank");
    static readonly FieldInfo s_loadedMusicWaveBank = AccessTools.Field(typeof(SoundSystem), "loadedMusicWaveBank");
    static readonly FieldInfo s_standbyWaveBanks_music = AccessTools.Field(typeof(SoundSystem), "dsxStandbyWaveBanks_Music");
    static readonly FieldInfo s_standbyWaveBanks_SFX = AccessTools.Field(typeof(SoundSystem), "dsxStandbyWaveBanks_SFX");
    static readonly FieldInfo s_songRegionMap = AccessTools.Field(typeof(SoundSystem), "dssSongRegionMap");
    static readonly FieldInfo s_soundSplitMap = AccessTools.Field(typeof(SoundSystem), "dssSoundSplitMap");
    static readonly FieldInfo s_universalMusic = AccessTools.Field(typeof(SoundSystem), "universalMusicWaveBank");
    static readonly FieldInfo s_effectStreamedWaveBank = AccessTools.Field(typeof(SoundSystem), "effectStreamedWaveBank");
    static readonly FieldInfo s_effectWaveBank = AccessTools.Field(typeof(SoundSystem), "effectWaveBank");
    static readonly FieldInfo s_effectSoundBank = AccessTools.Field(typeof(SoundSystem), "effectSoundBank");
    static readonly FieldInfo s_musicSoundBank = AccessTools.Field(typeof(SoundSystem), "musicSoundBank");
    static readonly FieldInfo s_audioEngine = AccessTools.Field(typeof(SoundSystem), "audioEngine");
    static readonly MethodInfo s_checkStandbyBanks = AccessTools.Method(typeof(SoundSystem), "CheckStandbyBanks");
    static readonly FieldInfo s_playingCuesOfSFXRegion = AccessTools.Field(typeof(SoundSystem), "dslcuePlayingCuesOfSFXRegion");
    static readonly FieldInfo s_sfxRegionsWaitingForUnload = AccessTools.Field(typeof(SoundSystem), "dsxSFXRegionsWaitingForUnload");
    static readonly FieldInfo s_streamingMusicBanks = AccessTools.Field(typeof(SoundSystem), "hsStreamingMusicBanks");

    public SoundSystem System { get; }

    public SoundSystemWrapper(SoundSystem system)
    {
        System = system;
    }

    public WaveBank CurrentMusicWaveBank
    {
        get => s_musicWaveBank.GetValue(System) as WaveBank;
        set => s_musicWaveBank.SetValue(System, value);
    }

    public WaveBank LoadedMusicWaveBank
    {
        get => s_loadedMusicWaveBank.GetValue(System) as WaveBank;
        set => s_loadedMusicWaveBank.SetValue(System, value);
    }

    public Dictionary<string, WaveBank> StandbyWaveBanksMusic
    {
        get => s_standbyWaveBanks_music.GetValue(System) as Dictionary<string, WaveBank>;
        set => s_standbyWaveBanks_music.SetValue(System, value);
    }

    public Dictionary<string, WaveBank> StandbyWaveBanksSFX
    {
        get => s_standbyWaveBanks_SFX.GetValue(System) as Dictionary<string, WaveBank>;
        set => s_standbyWaveBanks_SFX.SetValue(System, value);
    }

    public Dictionary<string, string> SongRegionMap
    {
        get => s_songRegionMap.GetValue(System) as Dictionary<string, string>;
        set => s_songRegionMap.SetValue(System, value);
    }

    public Dictionary<string, string> SoundSplitMap
    {
        get => s_soundSplitMap.GetValue(System) as Dictionary<string, string>;
        set => s_soundSplitMap.SetValue(System, value);
    }

    public WaveBank UniversalMusic
    {
        get => s_universalMusic.GetValue(System) as WaveBank;
        set => s_universalMusic.SetValue(System, value);
    }

    public WaveBank StreamedEffectWaveBank
    {
        get => s_effectStreamedWaveBank.GetValue(System) as WaveBank;
        set => s_effectStreamedWaveBank.SetValue(System, value);
    }

    public WaveBank EffectWaveBank
    {
        get => s_effectWaveBank.GetValue(System) as WaveBank;
        set => s_effectWaveBank.SetValue(System, value);
    }

    public Dictionary<string, List<SoundSystem.CueWrapper>> PlayingCuesOfSFXRegion
    {
        get => s_playingCuesOfSFXRegion.GetValue(System) as Dictionary<string, List<SoundSystem.CueWrapper>>;
        set => s_playingCuesOfSFXRegion.SetValue(System, value);
    }

    public AudioEngine AudioEngine
    {
        get => s_audioEngine.GetValue(System) as AudioEngine;
        set => s_audioEngine.SetValue(System, value);
    }

    public Dictionary<string, Utility.CInt> SFXRegionsWaitingForUnload
    {
        get => s_sfxRegionsWaitingForUnload.GetValue(System) as Dictionary<string, Utility.CInt>;
        set => s_sfxRegionsWaitingForUnload.SetValue(System, value);
    }

    public SoundBank EffectSoundBank
    {
        get => s_effectSoundBank.GetValue(System) as SoundBank;
        set => s_effectSoundBank.SetValue(System, value);
    }

    public SoundBank MusicSoundBank
    {
        get => s_musicSoundBank.GetValue(System) as SoundBank;
        set => s_musicSoundBank.SetValue(System, value);
    }

    public HashSet<string> StreamingMusicBanks
    {
        get => s_streamingMusicBanks.GetValue(System) as HashSet<string>;
        set => s_streamingMusicBanks.SetValue(System, value);
    }

    public void CheckStandbyBanks(string nextBankName)
    {
        s_checkStandbyBanks.Invoke(System, new object[] { nextBankName });
    }
}

