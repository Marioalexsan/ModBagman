namespace ModBagman;

/// <summary>
/// Registers a <see cref="CommandParser"/> instance or static method for automatic adding.
/// Use <see cref="CommandEntry.AutoAddModCommands"/> during loading to add such methods.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ModCommandAttribute : Attribute
{
    public ModCommandAttribute(string command = null)
    {
        Command = command;
    }

    /// <summary>
    /// String to use as command.
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// A short description of this command. Optional.
    /// </summary>
    public string Description { get; set; }
}
