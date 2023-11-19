using static SoG.SoundSystem;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), nameof(SoundSystem.AddNewCueToItsSoundRegion))]
class AddNewCueToItsSoundRegion
{
    static bool Prefix(string sCueName, CueWrapper cue)
    {
        var wSystem = new SoundSystemWrapper(Globals.Game.xSoundSystem);

        var entry = _Helper.GetAudioEntryFromID(sCueName);
        var name = _Helper.GetEffectName(sCueName);

        if (entry.EffectToWaveBank.TryGetValue(name, out string region))
        {
            if (!wSystem.PlayingCuesOfSFXRegion.ContainsKey(region))
                wSystem.PlayingCuesOfSFXRegion[region] = new List<CueWrapper>();

            wSystem.PlayingCuesOfSFXRegion[region].Add(cue);
        }

        return false;  // Never runs the original
    }
}
