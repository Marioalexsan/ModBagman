namespace ModBagman;

/// <summary>
/// Delegate that parses an incoming client message.
/// </summary>
/// <param name="msg"> The message data. </param>
/// <param name="connection"> The connection ID of the client. </param>
///
public delegate void ServerSideParser(BinaryReader msg, long connection);

/// <summary>
/// Delegate that parses an incoming server message.
/// </summary>
/// <param name="msg"> The message data. </param>
public delegate void ClientSideParser(BinaryReader msg);

/// <summary>
/// Contains data for sending custom packets between clients and servers.
/// </summary>
/// <remarks> 
/// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
/// </remarks>
[ModEntry(0)]
public class NetworkEntry : Entry<CustomEntryID.NetworkID>
{
    internal NetworkEntry() { }

    public Dictionary<ushort, ServerSideParser> ServerSide { get; } = new Dictionary<ushort, ServerSideParser>();

    public Dictionary<ushort, ClientSideParser> ClientSide { get; } = new Dictionary<ushort, ClientSideParser>();

    internal override void Initialize()
    {
        // Nothing to do
    }

    internal override void Cleanup()
    {
        // Nothing to do
    }
}
