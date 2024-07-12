using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace ModBagman;

public static class ContentManagerExtensions
{
    public const string ModContentPrefix = "ModContent://";

    private static readonly FieldInfo s_disposableAssetsField = AccessTools.Field(typeof(ContentManager), "disposableAssets");
    private static readonly FieldInfo s_loadedAssetsField = AccessTools.Field(typeof(ContentManager), "loadedAssets");
    private static readonly MethodInfo s_getCleanPathMethod = AccessTools.Method(AccessTools.TypeByName("Microsoft.Xna.Framework.TitleContainer"), "GetCleanPath");

    /// <summary>
    /// Returns true if the mod path starts with "ModContent\", false otherwise.
    /// </summary>
    public static bool IsModContentPath(this ContentManager _, string assetPath)
    {
        return assetPath != null && assetPath.Trim().Replace('/', '\\').StartsWith(ModContentPrefix);
    }

    /// <summary>
    /// Unloads a single asset from the given ContentManager.
    /// If the asset is found, it is disposed of, and removed from the ContentManager.
    /// </summary>
    /// <returns>True if asset was found and unloaded, false otherwise.</returns>
    public static bool Unload(this ContentManager manager, string path)
    {
        GetContentManagerFields(manager, out List<IDisposable> disposableAssets, out Dictionary<string, object> loadedAssets);

        var cleanPath = GetContentManagerCleanPath(path);

        if (loadedAssets.ContainsKey(cleanPath))
        {
            object asset = loadedAssets[cleanPath];

            loadedAssets.Remove(cleanPath);

            if (asset is IDisposable disposable)
            {
                disposableAssets.Remove(disposable);
                disposable.Dispose();
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public static T TryLoad<T>(this ContentManager manager, string path, bool useErrorAsset = false)
        where T : class
    {
        try
        {
            return manager.Load<T>(path);
        }
        catch (Exception e)
        {
            Program.Logger.LogWarning("Failed to load a {ResourceType}! Path: {Path}, Reason: {e}", typeof(T).Name, path, e.Message);
            return useErrorAsset ? GetErrorAsset<T>() : null;
        }
    }

    public static T LoadWithModSupport<T>(this ContentManager manager, string assetPath)
        where T : class
    {
        bool isModded = manager.IsModContentPath(assetPath);

        if (!isModded)
            return manager.Load<T>(assetPath);

        GetContentManagerFields(manager, out List<IDisposable> disposableAssets, out Dictionary<string, object> loadedAssets);

        string cleanedPath = Path.Combine(Globals.ModContentPath, assetPath[..ModContentPrefix.Length]);

        if (typeof(T) == typeof(Texture2D))
        {
            using var fileStream = File.OpenRead(cleanedPath);

            Texture2D texture = Texture2D.FromStream(Globals.Game.GraphicsDevice, fileStream);

            loadedAssets[assetPath] = texture;
            disposableAssets.Add(texture);

            return (T)(object)texture;
        }
        else
        {
            throw new NotImplementedException($"Loading modded assets of type {typeof(T).Name} is not supported.");
        }
    }

    public static T TryLoadWithModSupport<T>(this ContentManager manager, string assetPath, bool useErrorAsset = false)
        where T : class
    {
        try
        {
            return manager.LoadWithModSupport<T>(assetPath);
        }
        catch (Exception e)
        {
            Program.Logger.LogWarning("Failed to load a {ResourceType}! Path: {Path}, Reason: {e}", typeof(T).Name, assetPath, e.Message);
            return useErrorAsset ? GetErrorAsset<T>() : null;
        }
    }

    internal static void UnloadModContentPathAssets(this ContentManager manager)
    {
        GetContentManagerFields(manager, out List<IDisposable> disposableAssets, out Dictionary<string, object> loadedAssets);

        foreach (var kvp in loadedAssets.Where(x => manager.IsModContentPath(x.Key)).ToList())
        {
            loadedAssets.Remove(kvp.Key);

            if (kvp.Value is IDisposable disposable)
            {
                disposableAssets.Remove(disposable);
                disposable.Dispose();
            }
        }
    }

    internal static bool UnloadIfModded(this ContentManager manager, string path)
    {
        if (manager.IsModContentPath(path))
            return manager.Unload(path);

        return false;
    }

    private static void GetContentManagerFields(ContentManager manager, out List<IDisposable> disposableAssets, out Dictionary<string, object> loadedAssets)
    {
        disposableAssets = (List<IDisposable>)s_disposableAssetsField.GetValue(manager);

        loadedAssets = (Dictionary<string, object>)s_loadedAssetsField.GetValue(manager);
    }

    private static string GetContentManagerCleanPath(string path)
    {
        return (string)s_getCleanPathMethod.Invoke(null, new object[] { path });
    }

    private static T GetErrorAsset<T>()
        where T : class
    {
        if (typeof(T) == typeof(Texture2D))
            return ModBagmanResources.NullTexture as T;

        else return null;
    }
}
