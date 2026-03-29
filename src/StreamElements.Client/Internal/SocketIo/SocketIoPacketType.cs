namespace StreamElements.Client.Internal.SocketIo;

internal enum SocketIoPacketType
{
    Connect = 0,
    Disconnect = 1,
    Event = 2,
    Ack = 3,
    Error = 4,
    BinaryEvent = 5,
    BinaryAck = 6,
}
