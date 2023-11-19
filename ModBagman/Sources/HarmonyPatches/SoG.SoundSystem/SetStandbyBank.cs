using Microsoft.Xna.Framework.Audio;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), nameof(SoundSystem.SetStandbyBank))]
static class SetStandbyBank
{
    private static bool Prefix(string key, WaveBank bank)
    {
        SoundSystemWrapper wrapper = new SoundSystemWrapper(Globals.Game.xSoundSystem);

        if (bank != null && !bank.IsDisposed && bank != wrapper.UniversalMusic)
        {
            wrapper.StandbyWaveBanksMusic[key] = bank;
        }

        return false;
    }
}
