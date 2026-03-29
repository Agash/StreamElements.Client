namespace StreamElements.Client.Internal.SocketIo;

/// <summary>
/// Represents a parsed Socket.IO packet decoded from an Engine.IO message frame.
/// </summary>
internal sealed class SocketIoPacket
{
    public required SocketIoPacketType Type { get; init; }

    /// <summary>The raw text payload after the type digit (may be null for packets with no data).</summary>
    public string? Data { get; init; }
}
