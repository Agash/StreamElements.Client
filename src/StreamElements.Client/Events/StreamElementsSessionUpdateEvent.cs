using System.Text.Json;
using System.Text.Json.Serialization;

namespace StreamElements.Client.Events;

/// <summary>
/// Represents a session stat update event (event:update) from the StreamElements realtime endpoint.
/// Contains running session totals (tips total, followers today, etc.).
/// </summary>
public sealed record StreamElementsSessionUpdateEvent
{
    /// <summary>Gets the raw JSON data of the session update.</summary>
    [JsonPropertyName("data")]
    public JsonElement Data { get; init; }
}
