using System.Text.Json.Serialization;

namespace StreamElements.Client.Events;

/// <summary>
/// Represents a new subscriber or resubscription event received via StreamElements.
/// </summary>
public sealed record StreamElementsSubscriberEvent : StreamElementsRealtimeEvent
{
    /// <summary>Gets the subscriber event data.</summary>
    [JsonPropertyName("data")]
    public StreamElementsEventData? Data { get; init; }
}
