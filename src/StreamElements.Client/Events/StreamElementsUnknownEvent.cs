using System.Text.Json;
using System.Text.Json.Serialization;

namespace StreamElements.Client.Events;

/// <summary>
/// Wraps a StreamElements realtime event whose type is not recognised by this library.
/// </summary>
public sealed record StreamElementsUnknownEvent : StreamElementsRealtimeEvent
{
    /// <summary>Gets the raw JSON data from the event payload.</summary>
    [JsonPropertyName("data")]
    public JsonElement? Data { get; init; }
}
