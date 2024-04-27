using Microsoft.Xna.Framework.Audio;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), "_Update")]
static class _Update
{
    static void Postfix(SoundSystem __instance)
    {
        Globals.Game.lsDebugMafosoMaster.Add($"Current fade mod: {Globals.Game.xSoundSystem.xMusicVolumeMods.fIndoorsOutdorsFadeMod}");
        Globals.Game.lsDebugMafosoMaster.Add($"MusicVolume: {__instance.xMusicVolumeMods.fCurrentInGameMusicVolumeMod * 1.25f}");
        Globals.Game.lsDebugMafosoMaster.Add($"SoundVolume: {Globals.Game.xOptions.fGeneralSoundVolume * Globals.Game.xOptions.fGeneralSoundVolume * (Globals.Game.xOptions.fSoundVolume * Globals.Game.xOptions.fSoundVolume) * 5f}");
    }
}
