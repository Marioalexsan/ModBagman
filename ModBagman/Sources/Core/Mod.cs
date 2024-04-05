using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ModBagman;

/// <summary>
/// A list of supported implementations for mods.
/// </summary>
public enum ScriptEngine
{
    /// <summary>
    /// Uses native C# assemblies for mod code.
    /// </summary>
    CSharp,

    /// <summary>
    /// Scripting is done via JavaScript.
    /// </summary>
    JavaScript,

    /// <summary>
    /// Scripting is done via C# script files (.csx)
    /// </summary>
    CSharpScript
}

/// <summary>
/// The base class for all mods.
/// </summary>
/// <remarks>
/// Mod DLLs need to have at one class that is derived from <see cref="Mod"/>. That class will be constructed by ModBagman when loading.
/// </remarks>
public abstract partial class Mod
{
    private const int NameMinLength = 3;
    private const int NameMaxLength = 256;
    private const string NameRegexString = "^[a-zA-Z0-9_-]*$";
    private static readonly Regex NameRegex = new(NameRegexString);

    /// <summary>
    /// Builtin mods are first party mods that might need special treatment by the tool.
    /// </summary>
    internal virtual bool IsBuiltin => false;

    internal bool CompiledFromCSharp { get; set; } = false;

    /// <summary>
    /// Gets the script engine used for this mod.
    /// </summary>
    public ScriptEngine ScriptEngine
    {
        get
        {
            if (this is JavaScriptMod)
                return ScriptEngine.JavaScript;

            if (CompiledFromCSharp)
                return ScriptEngine.CSharpScript;

            return ScriptEngine.CSharp;
        }
    } 

    /// <summary>
    /// Gets the name of the mod. <para/>
    /// The name of a mod is used as an identifier, and should be unique between different mods!
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the version of the mod.
    /// <para> While not required, it would be a good idea to follow <see href="https://semver.org/">SemVer</see> conventions. </para> 
    /// </summary>
    public abstract Version Version { get; }

    /// <summary>
    /// Gets whenever the mod should have object creation disabled. <para/>
    /// Mods that have object creation disabled can't use methods such as <see cref="CreateItem(string)"/>. <para/>
    /// Additionally, mod information won't be sent in multiplayer or written in save files. <para/>
    /// This could be useful for mods that do not have a persistence component or 
    /// </summary>
    public virtual bool DisableObjectCreation => false;

    /// <summary>
    /// Gets the default logger for this mod.
    /// </summary>
    public ILogger Logger { get; protected set; } = Program.CreateLogFactory(false).CreateLogger("UnknownMod");

    /// <summary>
    /// Gets the path to the mod's assets, relative to the "ModContent" folder.
    /// The default value is "ModContent/{NameID}".
    /// </summary>
    public string AssetPath => Path.Combine(Globals.ModContentPath, Name) + "/";

    /// <summary>
    /// Gets the path from which the mod was loaded.
    /// This can be the path to an assembly, a zip file, or a development mode script folder.
    /// </summary>
    public string LoadedFrom { get; internal set; }

    /// <summary>
    /// Gets whenever the mod is currently being loaded.
    /// </summary>
    public bool InLoad => ModManager.CurrentlyLoadingMod == this;

    public Mod()
    {
    }

    /// <summary>
    /// Gets an active mod using its nameID.
    /// Returns null if the mod isn't currently loaded.
    /// </summary>
    /// <param name="nameID">The NameID of the mod.</param>
    /// <returns></returns>
    public Mod GetMod(string nameID)
    {
        return ModManager.Mods.FirstOrDefault(x => x.Name == nameID);
    }

    internal void ValidateAndSetup()
    {
        if (Name == null)
            throw new InvalidOperationException("Mod identifier cannot be null.");

        if (Name.Length < NameMinLength)
            throw new InvalidOperationException($"Mod identifier is way too short ({Name.Length} < {NameMinLength}).");

        if (Name.Length > NameMaxLength)
            throw new InvalidOperationException($"Mod identifier is way too long ({Name.Length} > {NameMaxLength}).");

        if (!NameRegex.IsMatch(Name))
            throw new InvalidOperationException($"Mod identifier {Name} cannot be used as an identifier. Names must follow the following regex: {NameRegexString}.");

        Logger = Program.CreateLogFactory(false).CreateLogger(Name);
    }
}