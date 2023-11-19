using Microsoft.Extensions.Logging;
using System.Security.Policy;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), nameof(SoundSystem.CheckForSoundRegion))]
static class CheckForSoundRegion
{
    static bool Prefix(ref SoundSystem __instance, string sCueName)
    {
        SoundSystemWrapper wSystem = new(__instance);

        AudioEntry entry = _Helper.GetAudioEntryFromID(sCueName);
        string name = _Helper.GetEffectName(sCueName);

        if (entry.EffectToWaveBank.TryGetValue(name, out string waveBank) && !wSystem.StandbyWaveBanksSFX.ContainsKey(waveBank))
        {
            __instance.LoadSFXBank(waveBank);
        }

        return false;
    }
}
