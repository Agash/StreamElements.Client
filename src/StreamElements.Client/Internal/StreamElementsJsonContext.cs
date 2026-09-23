using System.Text.Json;
using System.Text.Json.Serialization;
using StreamElements.Client.Events;
using StreamElements.Client.Internal.SocketIo;

namespace StreamElements.Client.Internal;

/// <summary>
/// Source-generated serialization metadata for every type the client reads or writes, so it stays
/// trim- and Native AOT-safe.
/// </summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(JsonElement[]))]
[JsonSerializable(typeof(SocketIoAuthPayload))]
[JsonSerializable(typeof(StreamElementsTipEvent))]
[JsonSerializable(typeof(StreamElementsSubscriberEvent))]
[JsonSerializable(typeof(StreamElementsCheerEvent))]
[JsonSerializable(typeof(StreamElementsFollowEvent))]
[JsonSerializable(typeof(StreamElementsHostEvent))]
[JsonSerializable(typeof(StreamElementsRaidEvent))]
[JsonSerializable(typeof(StreamElementsUnknownEvent))]
internal sealed partial class StreamElementsJsonContext : JsonSerializerContext;
