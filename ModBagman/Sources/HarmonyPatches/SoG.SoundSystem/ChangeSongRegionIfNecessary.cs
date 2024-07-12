using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Audio;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), nameof(SoundSystem.ChangeSongRegionIfNecessary))]
static class ChangeSongRegionIfNecessary
{
    static bool Prefix(ref SoundSystem __instance, string sSongName)
    {
        SoundSystemWrapper wSystem = new(__instance);

        var entry = _Helper.GetAudioEntryFromID(sSongName);
        string parsedName = _Helper.GetMusicName(sSongName);

        string nextBankName = "UniversalMusic";

        if (entry.MusicToWaveBank.TryGetValue(parsedName, out var bank))
            nextBankName = bank;

        string currentMusicBankName = wSystem.System.sCurrentMusicWaveBank;
        WaveBank currentMusicBank = wSystem.CurrentMusicWaveBank;

        bool currentIsUnloaded = currentMusicBank?.IsDisposed ?? true;

        if (nextBankName == currentMusicBankName && !currentIsUnloaded)
            return false;  // Wavebank is OK

        // Set current wavebank on standby if applicable
        if (!_Helper.IsPersistentWaveBank(currentMusicBankName))
        {
            Program.Logger.LogInformation("Setting music wavebank on standby: " + currentMusicBankName);
            wSystem.System.SetStandbyBank(currentMusicBankName, currentMusicBank);
        }

        bool isStreamed = _Helper.IsStreamedWaveBank(nextBankName);

        wSystem.System.sCurrentMusicWaveBank = nextBankName;

        // Set next wavebank
        if (_Helper.IsPersistentWaveBank(nextBankName))
        {
            // Persistent wavebank are simply set from the audio entries
            wSystem.CurrentMusicWaveBank = isStreamed ? entry.StreamedMusicWaveBank : entry.MusicWaveBank;
        }
        else
        {
            // Check if there's a bank on standby that could be ressurected
            if (wSystem.StandbyWaveBanksMusic.ContainsKey(nextBankName))
            {
                wSystem.CurrentMusicWaveBank = wSystem.StandbyWaveBanksMusic[nextBankName];
                wSystem.StandbyWaveBanksMusic.Remove(nextBankName);
            }
            else // We probably need to load the new bank
            {
                string root = entry.IsModded ? entry.Mod.AssetPath : Globals.Game.Content.RootDirectory;

                if (isStreamed)
                {
                    wSystem.LoadedMusicWaveBank = new WaveBank(wSystem.AudioEngine, $"{root}/Sound/{nextBankName}.xwb", 0, 16);
                }
                else
                {
                    wSystem.LoadedMusicWaveBank = new WaveBank(wSystem.AudioEngine, $"{root}/Sound/{nextBankName}.xwb");
                }
                wSystem.CurrentMusicWaveBank = null;
            }

            wSystem.System.xMusicVolumeMods.iMusicCueRetries = 0;
            wSystem.System.xMusicVolumeMods.sSongInWait = sSongName;
            wSystem.CheckStandbyBanks(nextBankName);
        }

        return false; // Never returns control to original
    }
}
