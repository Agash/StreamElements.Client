using StreamElements.Client.Internal;
using StreamElements.Client.Internal.SocketIo;

namespace StreamElements.Client.Tests.SocketIo;

[TestClass]
public sealed class SocketIoFramerTests
{
    // EIO open packet
    [TestMethod]
    public void TryParse_EngineIoOpenPacket_ReturnsConnectType()
    {
        const string frame =
            @"0{""sid"":""abc"",""upgrades"":[],""pingInterval"":25000,""pingTimeout"":5000}";
        SocketIoPacket? packet = SocketIoFramer.TryParse(frame);
        Assert.IsNotNull(packet);
        Assert.AreEqual(SocketIoPacketType.Connect, packet.Type);
        Assert.IsNotNull(packet.Data);
        Assert.Contains("sid", packet.Data, StringComparison.Ordinal);
    }

    // EIO ping — returns null (handled directly in receive loop)
    [TestMethod]
    public void TryParse_EngineIoPing_ReturnsNull()
    {
        SocketIoPacket? packet = SocketIoFramer.TryParse("2");
        Assert.IsNull(packet);
    }

    // Socket.IO connect confirmation
    [TestMethod]
    public void TryParse_SocketIoConnect_ReturnsConnectType()
    {
        SocketIoPacket? packet = SocketIoFramer.TryParse("40");
        Assert.IsNotNull(packet);
        Assert.AreEqual(SocketIoPacketType.Connect, packet.Type);
    }

    // Socket.IO event
    [TestMethod]
    public void TryParse_SocketIoEvent_ReturnsEventType()
    {
        const string frame = @"42[""authenticated"",{""channelId"":""ch1""}]";
        SocketIoPacket? packet = SocketIoFramer.TryParse(frame);
        Assert.IsNotNull(packet);
        Assert.AreEqual(SocketIoPacketType.Event, packet.Type);
        Assert.IsNotNull(packet.Data);
        Assert.Contains("authenticated", packet.Data, StringComparison.Ordinal);
    }

    // Socket.IO disconnect
    [TestMethod]
    public void TryParse_SocketIoDisconnect_ReturnsDisconnectType()
    {
        SocketIoPacket? packet = SocketIoFramer.TryParse("41");
        Assert.IsNotNull(packet);
        Assert.AreEqual(SocketIoPacketType.Disconnect, packet.Type);
    }

    // EIO close
    [TestMethod]
    public void TryParse_EngineIoClose_ReturnsDisconnectType()
    {
        SocketIoPacket? packet = SocketIoFramer.TryParse("1");
        Assert.IsNotNull(packet);
        Assert.AreEqual(SocketIoPacketType.Disconnect, packet.Type);
    }

    // Noop -> null
    [TestMethod]
    public void TryParse_EngineIoNoop_ReturnsNull()
    {
        SocketIoPacket? packet = SocketIoFramer.TryParse("6");
        Assert.IsNull(packet);
    }

    // Empty input -> null
    [TestMethod]
    public void TryParse_EmptyOrNull_ReturnsNull()
    {
        Assert.IsNull(SocketIoFramer.TryParse(""));
        Assert.IsNull(SocketIoFramer.TryParse(null));
    }

    // EncodeEvent
    [TestMethod]
    public void EncodeEvent_ProducesCorrectSocketIoEventFrame()
    {
        string frame = SocketIoFramer.EncodeEvent(
            "authenticate",
            new SocketIoAuthPayload("jwt", "tok"),
            StreamElementsJsonContext.Default.SocketIoAuthPayload
        );
        Assert.StartsWith("42[", frame, StringComparison.Ordinal);
        Assert.Contains("\"authenticate\"", frame, StringComparison.Ordinal);
        Assert.Contains("\"method\"", frame, StringComparison.Ordinal);
        Assert.Contains("\"jwt\"", frame, StringComparison.Ordinal);
    }

    // Pong constant
    [TestMethod]
    public void Pong_IsEngineIoPongFrame()
    {
        Assert.AreEqual("3", SocketIoFramer.Pong);
    }
}
