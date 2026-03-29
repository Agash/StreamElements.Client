using System.Text.Json.Serialization;

namespace StreamElements.Client.Events;

/// <summary>
/// Represents a new follower event received via StreamElements.
/// </summary>
public sealed record StreamElementsFollowEvent : StreamElementsRealtimeEvent
{
    /// <summary>Gets the follow event data.</summary>
    [JsonPropertyName("data")]
    public StreamElementsEventData? Data { get; init; }
}
