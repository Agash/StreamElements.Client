using System.Text.Json;

namespace StreamElements.Client.Internal.SocketIo;

internal static class SocketIoJsonOptions
{
    public static readonly JsonSerializerOptions Instance = new(JsonSerializerDefaults.Web);
}
