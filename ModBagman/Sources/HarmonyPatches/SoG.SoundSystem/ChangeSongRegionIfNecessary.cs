using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Audio;
using System.Web.UI.WebControls;
using static ModBagman.CustomEntryID;
using static SoG.SoundSystem;

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

        Program.Logger.LogInformation("Trying to load " + sSongName + " from " + nextBankName);

        if (nextBankName == currentMusicBankName)
            return false;  // We're in the same wavebank, changes shouldn't be needed

        // Set current wavebank on standby if applicable
        if (!_Helper.IsPersistentWaveBank(currentMusicBankName))
        {
            wSystem.System.SetStandbyBank(currentMusicBankName, currentMusicBank);
        }

        bool isStreamed = _Helper.IsStreamedWaveBank(nextBankName);

        // Set next wavebank
        if (_Helper.IsPersistentWaveBank(nextBankName))
        {
            // Persistent wavebank are simply set from the audio entries
            wSystem.CurrentMusicWaveBank = isStreamed ? entry.StreamedMusicWaveBank : entry.MusicWaveBank;
        }
        else
        {
            wSystem.System.sCurrentMusicWaveBank = nextBankName;

            // Check if there's a bank on standby that could be ressurected
            if (wSystem.StandbyWaveBanksMusic.ContainsKey(nextBankName))
            {
                wSystem.CurrentMusicWaveBank = wSystem.StandbyWaveBanksMusic[nextBankName];
                wSystem.StandbyWaveBanksMusic.Remove(nextBankName);
            }
            else // We probably need to load the new bank
            {
                string root = entry.IsModded ? entry.Mod.AssetPath : Globals.Game.Content.RootDirectory;

                Program.Logger.LogInformation("Path " + $"{root}/Sound/{nextBankName}.xwb " + isStreamed);

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
