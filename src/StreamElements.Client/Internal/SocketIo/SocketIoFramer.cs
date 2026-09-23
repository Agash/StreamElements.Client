using System.Text.Json;

namespace StreamElements.Client.Internal.SocketIo;

/// <summary>
/// Encodes and decodes Engine.IO v3 / Socket.IO v2 text frames.
/// </summary>
/// <remarks>
/// Frame format (text):
///   Engine.IO packet: single digit type prefix + payload
///     0{...}   = open (JSON sid/pingInterval/etc)
///     2        = ping  (server to client in EIO v3)
///     3        = pong  (client to server in EIO v3)
///     4&lt;sio&gt;  = message containing a Socket.IO packet
///
///   Socket.IO packet (payload of EIO message):
///     40       = Socket.IO CONNECT on default namespace
///     42[...]  = Socket.IO EVENT — JSON array of [eventName, data]
/// </remarks>
internal static class SocketIoFramer
{
    /// <summary>
    /// Encodes a Socket.IO EVENT packet (EIO message wrapping a Socket.IO event).
    /// Result: "42[&lt;eventName&gt;,&lt;jsonData&gt;]"
    /// </summary>
    public static string EncodeEvent(string eventName, object data)
    {
        ArgumentException.ThrowIfNullOrEmpty(eventName);

        string json = JsonSerializer.Serialize(data, SocketIoJsonOptions.Instance);
        return $"42[\"{eventName}\",{json}]";
    }

    /// <summary>
    /// Encodes a raw Socket.IO EVENT packet with a pre-serialized JSON array payload.
    /// </summary>
    public static string EncodeEventRaw(string eventName, string jsonPayload)
    {
        ArgumentException.ThrowIfNullOrEmpty(eventName);
        ArgumentException.ThrowIfNullOrEmpty(jsonPayload);

        return $"42[\"{eventName}\",{jsonPayload}]";
    }

    /// <summary>Returns the EIO pong frame text.</summary>
    public static string Pong => "3";

    /// <summary>
    /// Tries to parse an Engine.IO text frame into a SocketIoPacket.
    /// Returns null for unrecognised/unsupported frames (including EIO ping — handle "2" directly in the receive loop).
    /// </summary>
    public static SocketIoPacket? TryParse(string? frame)
    {
        if (string.IsNullOrEmpty(frame))
        {
            return null;
        }

        // First char is the EIO packet type.
        if (!int.TryParse(frame[..1], out int eioType))
        {
            return null;
        }

        EngineIoPacketType eio = (EngineIoPacketType)eioType;

        return eio switch
        {
            EngineIoPacketType.Ping => null, // handled directly in the receive loop
            EngineIoPacketType.Pong => null,
            EngineIoPacketType.Noop => null,
            EngineIoPacketType.Open => new SocketIoPacket
            {
                Type = SocketIoPacketType.Connect,
                Data = frame[1..],
            },
            EngineIoPacketType.Close => new SocketIoPacket
            {
                Type = SocketIoPacketType.Disconnect,
                Data = null,
            },
            EngineIoPacketType.Message => ParseSocketIoPacket(frame[1..]),
            _ => null,
        };
    }

    private static SocketIoPacket? ParseSocketIoPacket(string payload)
    {
        if (string.IsNullOrEmpty(payload) || !int.TryParse(payload[..1], out int sioType))
        {
            return null;
        }

        SocketIoPacketType type = (SocketIoPacketType)sioType;
        string? data = payload.Length > 1 ? payload[1..] : null;

        return new SocketIoPacket { Type = type, Data = data };
    }
}
