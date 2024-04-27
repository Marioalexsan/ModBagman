using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Audio;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), nameof(SoundSystem.LoadStandbyBank))]
static class LoadStandbyBank
{
    static bool Prefix(string sStandbyBank)
    {
        var wSystem = new SoundSystemWrapper(Globals.Game.xSoundSystem);

        if (wSystem.System.bSystemActive && !wSystem.StandbyWaveBanksMusic.ContainsKey(sStandbyBank))
        {
            // Need to search through all music banks

            AudioEntry entry = Entries.Audio.FirstOrDefault(x => x.MusicToWaveBank.ContainsValue(sStandbyBank)) ?? ModManager.Vanilla.GetAudio();

            string root = entry.IsModded ? entry.Mod.AssetPath : Globals.Game.Content.RootDirectory;

            if (_Helper.IsStreamedWaveBank(sStandbyBank))
            {
                Program.Logger.LogInformation("Loading streamed music bank " + sStandbyBank);
                wSystem.System.SetStandbyBank(sStandbyBank, new WaveBank(wSystem.AudioEngine, $"{root}/Sound/{sStandbyBank}.xwb", 0, 16));
            }
            else
            {
                Program.Logger.LogInformation("Loading non-streamed music bank " + sStandbyBank);
                wSystem.System.SetStandbyBank(sStandbyBank, new WaveBank(wSystem.AudioEngine, $"{root}/Sound/{sStandbyBank}.xwb"));
            }
        }
        return false;
    }
}
