using System.Text.Json.Serialization;

namespace StreamElements.Client.Events;

/// <summary>
/// Represents a tip (donation) received via StreamElements.
/// </summary>
public sealed record StreamElementsTipEvent : StreamElementsRealtimeEvent
{
    /// <summary>Gets the tip event data.</summary>
    [JsonPropertyName("data")]
    public StreamElementsEventData? Data { get; init; }
}
