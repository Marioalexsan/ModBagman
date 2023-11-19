using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Audio;
using System.Web.UI.WebControls;
using static ModBagman.CustomEntryID;
using static SoG.HitEffectMap;
using static SoG.RogueLikeMode;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), nameof(SoundSystem.LoadSFXBank))]
static class LoadSFXBank
{
    static bool Prefix(SoundSystem __instance, string sName)
    {
        SoundSystemWrapper wSystem = new(__instance);

        AudioEntry entry = Entries.Audio.FirstOrDefault(x => x.EffectToWaveBank.ContainsValue(sName)) ?? ModManager.Vanilla.GetAudio();

        string root = entry.IsModded ? entry.Mod.AssetPath : Globals.Game.Content.RootDirectory;

        Program.Logger.LogInformation(sName);

        if (wSystem.System.bSystemActive)
        {
            if (wSystem.SFXRegionsWaitingForUnload.ContainsKey(sName))
            {
                wSystem.SFXRegionsWaitingForUnload.Remove(sName);
            }
            if (!wSystem.StandbyWaveBanksSFX.ContainsKey(sName))
            {
                wSystem.StandbyWaveBanksSFX[sName] = new WaveBank(wSystem.AudioEngine, root + "/Sound/" + sName + ".xwb");
            }
        }

        return false;
    }
}
