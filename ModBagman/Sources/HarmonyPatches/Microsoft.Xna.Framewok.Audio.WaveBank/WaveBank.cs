using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Audio;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch]
internal static class Microsoft_Xna_Framework_Audio_Wavebank
{
    [HarmonyPatch(typeof(WaveBank), MethodType.Constructor, typeof(AudioEngine), typeof(string))]
    [HarmonyPrefix]
    static void NonStreamingConstructor(string nonStreamingWaveBankFilename)
    {
        Program.Logger.LogInformation("Loading XACT non-streaming wavebank from " + nonStreamingWaveBankFilename);
    }

    [HarmonyPatch(typeof(WaveBank), MethodType.Constructor, typeof(AudioEngine), typeof(string), typeof(int), typeof(short))]
    [HarmonyPrefix]
    static void StreamingConstructor(string streamingWaveBankFilename)
    {
        Program.Logger.LogInformation("Loading XACT streaming wavebank from " + streamingWaveBankFilename);
    }

    [HarmonyPatch(typeof(WaveBank), MethodType.Constructor, typeof(AudioEngine), typeof(string))]
    [HarmonyFinalizer]
    static void NonStreamingConstructorFinalizer(string nonStreamingWaveBankFilename, Exception __exception)
    {
        if (__exception != null)
            Program.Logger.LogInformation("Failed to load XACT wavebank from " + nonStreamingWaveBankFilename);
    }

    [HarmonyPatch(typeof(WaveBank), MethodType.Constructor, typeof(AudioEngine), typeof(string), typeof(int), typeof(short))]
    [HarmonyFinalizer]
    static void StreamingConstructor(string streamingWaveBankFilename, Exception __exception)
    {
        if (__exception != null)
            Program.Logger.LogInformation("Failed to load XACT wavebank from " + streamingWaveBankFilename);
    }

    [HarmonyPatch(typeof(WaveBank), "Dispose", typeof(bool))]
    [HarmonyPrefix]
    static void Dispose()
    {
        Program.Logger.LogInformation("XACT wavebank just got disposed");
    }
}
