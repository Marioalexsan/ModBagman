using Microsoft.Xna.Framework.Audio;

namespace ModBagman;

public static class AudioEngineExtensions
{
    public static WaveBank TryLoadWaveBank(this AudioEngine engine, string path, bool streamed)
    {
        engine.TryLoadWaveBank(path, out WaveBank asset, streamed);
        return asset;
    }

    public static bool TryLoadWaveBank(this AudioEngine engine, string path, out WaveBank result, bool streamed)
    {
        try
        {
            result = streamed ? new WaveBank(engine, path) : new WaveBank(engine, path, 0, 16);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    public static SoundBank TryLoadSoundBank(this AudioEngine engine, string path)
    {
        engine.TryLoadSoundBank(path, out SoundBank asset);
        return asset;
    }

    public static bool TryLoadSoundBank(this AudioEngine engine, string path, out SoundBank result)
    {
        try
        {
            result = new SoundBank(engine, path);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }
}
