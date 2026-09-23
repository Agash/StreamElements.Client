using StreamElements.Client.Internal.SocketIo;
using Xunit;

namespace StreamElements.Client.Tests.SocketIo;

public sealed class SocketIoFramerTests
{
    // EIO open packet
    [Fact]
    public void TryParse_EngineIoOpenPacket_ReturnsConnectType()
    {
        const string frame =
            @"0{""sid"":""abc"",""upgrades"":[],""pingInterval"":25000,""pingTimeout"":5000}";
        SocketIoPacket? packet = SocketIoFramer.TryParse(frame);
        Assert.NotNull(packet);
        Assert.Equal(SocketIoPacketType.Connect, packet.Type);
        Assert.Contains("sid", packet.Data);
    }

    // EIO ping — returns null (handled directly in receive loop)
    [Fact]
    public void TryParse_EngineIoPing_ReturnsNull()
    {
        SocketIoPacket? packet = SocketIoFramer.TryParse("2");
        Assert.Null(packet);
    }

    // Socket.IO connect confirmation
    [Fact]
    public void TryParse_SocketIoConnect_ReturnsConnectType()
    {
        SocketIoPacket? packet = SocketIoFramer.TryParse("40");
        Assert.NotNull(packet);
        Assert.Equal(SocketIoPacketType.Connect, packet.Type);
    }

    // Socket.IO event
    [Fact]
    public void TryParse_SocketIoEvent_ReturnsEventType()
    {
        const string frame = @"42[""authenticated"",{""channelId"":""ch1""}]";
        SocketIoPacket? packet = SocketIoFramer.TryParse(frame);
        Assert.NotNull(packet);
        Assert.Equal(SocketIoPacketType.Event, packet.Type);
        Assert.Contains("authenticated", packet.Data);
    }

    // Socket.IO disconnect
    [Fact]
    public void TryParse_SocketIoDisconnect_ReturnsDisconnectType()
    {
        SocketIoPacket? packet = SocketIoFramer.TryParse("41");
        Assert.NotNull(packet);
        Assert.Equal(SocketIoPacketType.Disconnect, packet.Type);
    }

    // EIO close
    [Fact]
    public void TryParse_EngineIoClose_ReturnsDisconnectType()
    {
        SocketIoPacket? packet = SocketIoFramer.TryParse("1");
        Assert.NotNull(packet);
        Assert.Equal(SocketIoPacketType.Disconnect, packet.Type);
    }

    // Noop -> null
    [Fact]
    public void TryParse_EngineIoNoop_ReturnsNull()
    {
        SocketIoPacket? packet = SocketIoFramer.TryParse("6");
        Assert.Null(packet);
    }

    // Empty input -> null
    [Fact]
    public void TryParse_EmptyOrNull_ReturnsNull()
    {
        Assert.Null(SocketIoFramer.TryParse(""));
        Assert.Null(SocketIoFramer.TryParse(null));
    }

    // EncodeEvent
    [Fact]
    public void EncodeEvent_ProducesCorrectSocketIoEventFrame()
    {
        string frame = SocketIoFramer.EncodeEvent(
            "authenticate",
            new { method = "jwt", token = "tok" }
        );
        Assert.StartsWith("42[", frame, StringComparison.Ordinal);
        Assert.Contains("\"authenticate\"", frame, StringComparison.Ordinal);
        Assert.Contains("\"method\"", frame, StringComparison.Ordinal);
        Assert.Contains("\"jwt\"", frame, StringComparison.Ordinal);
    }

    // Pong constant
    [Fact]
    public void Pong_IsEngineIoPongFrame()
    {
        Assert.Equal("3", SocketIoFramer.Pong);
    }
}
