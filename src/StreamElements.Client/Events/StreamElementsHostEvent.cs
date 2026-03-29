using System.Text.Json.Serialization;

namespace StreamElements.Client.Events;

/// <summary>
/// Represents a channel host event received via StreamElements.
/// </summary>
public sealed record StreamElementsHostEvent : StreamElementsRealtimeEvent
{
    /// <summary>Gets the host event data.</summary>
    [JsonPropertyName("data")]
    public StreamElementsEventData? Data { get; init; }
}
