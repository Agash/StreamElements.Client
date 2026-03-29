using System.Text.Json.Serialization;

namespace StreamElements.Client.Events;

/// <summary>
/// Represents a raid event received via StreamElements.
/// </summary>
public sealed record StreamElementsRaidEvent : StreamElementsRealtimeEvent
{
    /// <summary>Gets the raid event data.</summary>
    [JsonPropertyName("data")]
    public StreamElementsEventData? Data { get; init; }
}
