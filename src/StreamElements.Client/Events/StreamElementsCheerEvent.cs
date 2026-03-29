using System.Text.Json.Serialization;

namespace StreamElements.Client.Events;

/// <summary>
/// Represents a Twitch bits cheer received via StreamElements.
/// </summary>
public sealed record StreamElementsCheerEvent : StreamElementsRealtimeEvent
{
    /// <summary>Gets the cheer event data.</summary>
    [JsonPropertyName("data")]
    public StreamElementsEventData? Data { get; init; }
}
