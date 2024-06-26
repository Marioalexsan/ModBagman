﻿using Microsoft.Extensions.Logging;
using System.Reflection.Emit;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), nameof(SoundSystem.PlaySong))]
static class PlaySong
{
    static void Prefix(ref string sSongName, bool bFadeIn)
    {
        var redirects = AudioEntry.MusicRedirects;
        string audioIDToUse = sSongName;

        if (redirects.ContainsKey(audioIDToUse))
            audioIDToUse = redirects[audioIDToUse];

        Program.Logger.LogInformation($"Playing music {audioIDToUse} ({sSongName})");
        sSongName = audioIDToUse;
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code, ILGenerator gen)
        => ReadySongInCue.Transpiler(code, gen);
}
