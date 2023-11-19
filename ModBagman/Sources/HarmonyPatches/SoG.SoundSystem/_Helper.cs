using Microsoft.Xna.Framework.Audio;

namespace ModBagman.HarmonyPatches;

static class _Helper
{
    internal static SoundBank GetEffectSoundBank(string effectID)
    {
        var entry = GetAudioEntryFromID(effectID);
        return entry.EffectsSoundBank;
    }

    internal static SoundBank GetMusicSoundBank(string effectID)
    {
        var entry = GetAudioEntryFromID(effectID);
        return entry.MusicSoundBank;
    }

    internal static AudioEntry GetAudioEntryFromID(string effectID)
    {
        if (!AudioEntry.ModAudioID.TryParse(effectID, out var id))
            return Entries.Audio.Get(ModManager.Vanilla, "");

        return Entries.Audio.Get((CustomEntryID.AudioID)id.ModIndex);
    }

    internal static string GetEffectName(string effectID)
    {
        if (!AudioEntry.ModAudioID.TryParse(effectID, out var id))
            return effectID;  // Pray that this is a vanilla effect

        var entry = Entries.Audio.Get((CustomEntryID.AudioID)id.ModIndex);

        return entry?.Effects.ElementAtOrDefault(id.AudioIndex);
    }

    internal static string GetMusicName(string audioID)
    {
        if (!AudioEntry.ModAudioID.TryParse(audioID, out var id))
            return audioID;  // Pray that this is a vanilla music

        var entry = Entries.Audio.Get((CustomEntryID.AudioID)id.ModIndex);

        return entry?.Music.ElementAtOrDefault(id.AudioIndex);
    }

    internal static bool IsStreamedWaveBank(string bank)
    {
        // Handle some cases for vanilla
        if (bank == "StreamedEffects")
            return true;

        if (bank == "UniversalMusic")
            return true;

        // Handle modded cases
        return ModManager.Mods.Any(mod =>
        {
            if (bank == mod.Name + "_StreamedEffects")
                return true;

            if (bank == mod.Name + "_StreamedMusic")
                return true;

            AudioEntry entry = mod.GetAudio();

            if (entry != null && entry.MusicWaveBanksToStream.Any(x => x == bank))
                return true;

            return false;
        });
    }

    internal static bool IsPersistentWaveBank(string bank)
    {
        // Handle some cases for vanilla
        if (bank == "Effects")
            return true;

        if (bank == "StreamedEffects")
            return true;

        if (bank == "UniversalMusic")
            return true;

        // Handle modded cases
        return ModManager.Mods.Any(mod =>
        {
            if (bank == mod.Name + "_Music")
                return true;

            if (bank == mod.Name + "_Effects")
                return true;

            if (bank == mod.Name + "_StreamedEffects")
                return true;

            if (bank == mod.Name + "_StreamedMusic")
                return true;

            return false;
        });
    }

    internal static bool IsPersistentWaveBank(WaveBank bank)
    {
        var wSystem = new SoundSystemWrapper(Globals.Game.xSoundSystem);

        // Handle some cases for vanilla
        if (bank == wSystem.UniversalMusic)
            return true;

        if (bank == wSystem.StreamedEffectWaveBank)
            return true;

        if (bank == wSystem.EffectWaveBank)
            return true;

        // Handle modded cases
        return ModManager.Mods.Any(mod =>
        {
            var entry = Entries.Audio.Get(mod, "");

            if (entry == null)
                return false;

            if (bank == entry.EffectsWaveBank)
                return true;

            if (bank == entry.MusicWaveBank)
                return true;

            if (bank == entry.StreamedEffectsWaveBank)
                return true;

            if (bank == entry.StreamedMusicWaveBank)
                return true;

            return false;
        });
    }
}