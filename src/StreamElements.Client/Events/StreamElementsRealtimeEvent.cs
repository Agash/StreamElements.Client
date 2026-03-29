using System.Text.Json.Serialization;

namespace StreamElements.Client.Events;

/// <summary>
/// Base record for all events received from the StreamElements realtime endpoint.
/// </summary>
public abstract record StreamElementsRealtimeEvent
{
    /// <summary>Gets the StreamElements internal event ID.</summary>
    [JsonPropertyName("_id")]
    public string? Id { get; init; }

    /// <summary>Gets the channel ID this event belongs to.</summary>
    [JsonPropertyName("channel")]
    public string? Channel { get; init; }

    /// <summary>Gets the event type string (e.g. "tip", "subscriber").</summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>Gets the upstream provider (e.g. "twitch", "youtube").</summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; init; }

    /// <summary>Gets whether this event was flagged/moderated.</summary>
    [JsonPropertyName("flagged")]
    public bool? Flagged { get; init; }

    /// <summary>Gets the ISO 8601 timestamp when this event was created.</summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>Gets the ISO 8601 timestamp when this event was last updated.</summary>
    [JsonPropertyName("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; init; }
}
