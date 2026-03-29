namespace StreamElements.Client.Internal.SocketIo;

internal enum EngineIoPacketType
{
    Open = 0,
    Close = 1,
    Ping = 2,
    Pong = 3,
    Message = 4,
    Upgrade = 5,
    Noop = 6,
}
