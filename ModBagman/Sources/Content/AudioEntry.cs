using Microsoft.Xna.Framework.Audio;
using Microsoft.Extensions.Logging;
using static SoG.HitEffectMap;

namespace ModBagman;

/// <summary>
/// Contains data for playing custom audio files inside the game.
/// Each mod can have at most one audio entry. <para/>
/// Audio files are loaded from a preset "Sound" folder inside the mod's content path. <para/>
/// You can use XACT3 to generate wave banks and sound banks to use in your mods. <para/>
/// </summary>
/// <remarks>
/// ModBagman uses preset wave bank and sound bank names to load audio data.
/// Depending on what you do, you will need the following wave banks in the "Sound" folder:
/// <list type="bullet">
///     <item>
///         <term>{<see cref="Mod.Name"/>}Effects.xwb</term>
///         <description>The universal effect wave bank that will persist in memory until exit</description>
///     </item>
///     <item>
///         <term>{<see cref="Mod.Name"/>}Effects{Region}.xwb</term>
///         <description>A regional effect wave bank that will be loaded on demand</description>
///     </item>
///     <item>
///         <term>{<see cref="Mod.Name"/>}Effects.xsb</term>
///         <description>The sound bank used for effects</description>
///     </item>
///     <item>
///         <term>{<see cref="Mod.Name"/>}Music.xwb</term>
///         <description>The universal music wave bank that will persist in memory until exit</description>
///     </item>
///     <item>
///         <term>{<see cref="Mod.Name"/>}Music{Region}.xwb</term>
///         <description>A regional effect wave bank that will be loaded on demand</description>
///     </item>
///     <item>
///         <term>{<see cref="Mod.Name"/>}Music.xsb</term>
///         <description>The sound bank used for music</description>
///     </item>
/// </list> 
/// </remarks>
/// <remarks> 
/// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
/// </remarks>
[ModEntry(0)]
public class AudioEntry : Entry<CustomEntryID.AudioID>
{
    internal AudioEntry() { }

    public readonly struct ModAudioID
    {
        public ModAudioID(int modIndex, int cueID)
        {
            ModIndex = modIndex;
            AudioIndex = cueID;
        }

        public readonly int ModIndex;
        public readonly int AudioIndex;

        public static bool TryParse(string str, out ModAudioID audioID)
        {
            audioID = default;

            if (!str.StartsWith("Mod_"))
                return false;

            string[] words = str.Remove(0, 4).Split('_');

            if (words.Length != 2)
                return false;

            if (!int.TryParse(words[0], out int modIndex) || !int.TryParse(words[1], out int cueID))
                return false;

            audioID = new ModAudioID(modIndex, cueID);
            return true;
        }

        public override string ToString() => $"Mod_{ModIndex}_{AudioIndex}";
    }

    internal static Dictionary<string, string> MusicRedirects { get; } = new Dictionary<string, string>();

    internal SoundBank EffectsSoundBank;       // "<Mod>_Effects.xsb"
    internal WaveBank EffectsWaveBank;         // "<Mod>_Effects.xwb"
    internal WaveBank StreamedEffectsWaveBank; // "<Mod>_StreamedEffects.xwb"

    internal SoundBank MusicSoundBank;         // "<Mod>_Music.xsb"
    internal WaveBank MusicWaveBank;           // "<Mod>_Music.xwb"
    internal WaveBank StreamedMusicWaveBank;   // "<Mod>_StreamedMusic.xwb"

    // All of the cues that have been declared by the mod
    internal List<string> Effects { get; } = new();
    internal List<string> Music { get; } = new();

    // Mapping of the cues to their wave banks
    internal Dictionary<string, string> EffectToWaveBank { get; } = new();
    internal Dictionary<string, string> MusicToWaveBank { get; } = new();

    // All of the *regional* wavebanks that will be loaded in streaming mode
    internal List<string> MusicWaveBanksToStream { get; } = new();

    public void SetMusicBankStreamingMode(string bankName, bool useStreaming)
    {
        bool currentlyUseStreaming = MusicWaveBanksToStream.Contains(bankName);

        if (!currentlyUseStreaming && useStreaming)
        {
            MusicWaveBanksToStream.Add(bankName);
        }
        else if (currentlyUseStreaming && !useStreaming)
        {
            MusicWaveBanksToStream.Remove(bankName);
        }
    }

    /// <summary>
    /// Adds effect cues for this mod. The cues be loaded using the given wave bank.
    /// Keep in mind that the universal effect wave bank follows a "never unload" policy.
    /// </summary>
    /// <remarks>
    /// This method can only be used inside <see cref="Mod.Load"/>.
    /// </remarks>
    /// <param name="bankName"> The wave bank name containing the effects (without the ".xnb" extension). </param>
    /// <param name="cues"> A list of effect names to add. </param>
    public void AddEffectsFromWaveBank(string bankName, params string[] cues)
    {
        ErrorHelper.ThrowIfNotLoading(Mod);

        if (!bankName.StartsWith(Mod.Name + "_Effects"))
            throw new ArgumentException("Bank name should follow the pattern \"{Mod}_Effects{Region}\"");

        foreach (var cue in cues)
        {
            if (!Effects.Contains(cue))
                Effects.Add(cue);
            EffectToWaveBank[cue] = bankName;
        }
    }

    /// <summary>
    /// Adds music cues for this mod. The cues be loaded using the given wave bank.
    /// Keep in mind that the universal music wave bank follows a "never unload" policy.
    /// </summary>
    /// <remarks>
    /// This method can only be used inside <see cref="Mod.Load"/>.
    /// </remarks>
    /// <param name="bankName"> The wave bank name containing the music (without the ".xnb" extension). </param>
    /// <param name="cues"> A list of music names to add. </param>
    public void AddMusicFromWaveBank(string bankName, params string[] cues)
    {
        ErrorHelper.ThrowIfNotLoading(Mod);

        if (!bankName.StartsWith(Mod.Name + "_Music"))
            throw new ArgumentException("Bank name should follow the pattern \"{Mod}_Music{Region}\"");

        foreach (var cue in cues)
        {
            if (!Music.Contains(cue))
                Music.Add(cue);
            MusicToWaveBank[cue] = bankName;
        }
    }

    /// <summary>
    /// Gets the ID of the effect that has the given name. <para/>
    /// This ID can be used to play effects with methods such as <see cref="SoundSystem.PlayCue"/>.
    /// </summary>
    /// <returns> An identifier that can be used to play the effect using vanilla methods. </returns>
    public ModAudioID? GetEffectID(string effectName)
    {
        int index = Effects.IndexOf(effectName);

        if (index == -1)
            return null;

        return new ModAudioID((int)GameID, index);
    }

    /// <summary>
    /// Gets the ID of the music that has the given cue name. <para/>
    /// This ID can be used to play music with <see cref="SoundSystem.PlaySong"/>.
    /// </summary>
    /// <returns> An identifier that can be used to play music using vanilla methods. </returns>
    public ModAudioID? GetMusicID(string musicName)
    {
        int index = Music.IndexOf(musicName);

        if (index == -1)
            return null;

        return new ModAudioID((int)GameID, index);
    }

    /// <summary>
    /// Redirects a music to another one - whenever the first music would play
    /// normally, the other music will play instead.
    /// This redirection is only applied once, i.e. you can't stack redirections.
    /// </summary>
    /// <remarks>
    /// This method cannot be used during <see cref="Mod.Load"/>.
    /// Use it somewhere else, such as <see cref="Mod.PostLoad"/>.
    /// </remarks>
    /// <param name="inboundMusic"> The vanilla music to redirect from. </param>
    /// <param name="outboundMusic"> The modded music to redirect to. Set to null to remove redirect. </param>
    public void RedirectMusic(string inboundMusic, string outboundMusic)
    {
        ErrorHelper.ThrowIfLoading(Mod);

        if (outboundMusic == null)
        {
            Program.Logger.LogWarning("Removed music redirect for {inboundMusic}.", inboundMusic);
            MusicRedirects.Remove(inboundMusic);
            return;
        }
        else
        {
            Program.Logger.LogWarning("Set music redirect {inboundMusic} -> {outboundMusic}", inboundMusic, outboundMusic);
            MusicRedirects[inboundMusic] = outboundMusic;
        }
    }

    protected override void Initialize()
    {
        var audioEngine = new SoundSystemWrapper(Globals.Game.xSoundSystem).AudioEngine;

        string root = IsModded ? Mod.AssetPath : Globals.Game.Content.RootDirectory;

        string name = Mod.Name;

        if (IsModded)
        {
            // Non-unique sound / wave banks will cause audio conflicts
            // This is why the file paths are set in stone

            EffectsSoundBank = audioEngine.TryLoadSoundBank(Path.Combine(root, $"Sound/{name}_Effects.xsb"));
            MusicSoundBank = audioEngine.TryLoadSoundBank(Path.Combine(root, $"Sound/{name}_Music.xsb"));

            EffectsWaveBank = audioEngine.TryLoadWaveBank(Path.Combine(root, $"Sound/{name}_Effects.xwb"), false);
            MusicWaveBank = audioEngine.TryLoadWaveBank(Path.Combine(root, $"Sound/{name}_Music.xwb"), false);

            StreamedEffectsWaveBank = audioEngine.TryLoadWaveBank(Path.Combine(root, $"Sound/{name}_StreamedEffects.xwb"), true);
            StreamedMusicWaveBank = audioEngine.TryLoadWaveBank(Path.Combine(root, $"Sound/{name}_StreamedMusic.xwb"), true);
        }
    }

    protected override void Cleanup()
    {
        if (IsModded)
        {
            EffectsSoundBank?.Dispose();
            MusicSoundBank?.Dispose();

            EffectsWaveBank?.Dispose();
            MusicWaveBank?.Dispose();

            StreamedEffectsWaveBank?.Dispose();
            StreamedMusicWaveBank?.Dispose();
        }
    }
}
