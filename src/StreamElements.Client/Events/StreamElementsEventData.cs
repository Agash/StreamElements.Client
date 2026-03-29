using System.Text.Json.Serialization;

namespace StreamElements.Client.Events;

/// <summary>
/// Common data payload shared across StreamElements realtime events.
/// </summary>
public sealed record StreamElementsEventData
{
    /// <summary>Gets the username (login name) of the actor.</summary>
    [JsonPropertyName("username")]
    public string? Username { get; init; }

    /// <summary>Gets the display name of the actor.</summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; init; }

    /// <summary>Gets the numeric amount (bits cheered, months subbed, tip amount, raiders, viewers hosted, etc.).</summary>
    [JsonPropertyName("amount")]
    public decimal? Amount { get; init; }

    /// <summary>Gets the currency code for tip events (ISO 4217).</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    /// <summary>Gets the user message attached to the event.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>Gets the subscription tier string ("1000", "2000", "3000", or "prime").</summary>
    [JsonPropertyName("tier")]
    public string? Tier { get; init; }

    /// <summary>Gets the current subscription streak in months.</summary>
    [JsonPropertyName("streak")]
    public int? Streak { get; init; }

    /// <summary>Gets the quantity for gifted subscriptions.</summary>
    [JsonPropertyName("quantity")]
    public int? Quantity { get; init; }

    /// <summary>Gets whether this is a gifted subscription.</summary>
    [JsonPropertyName("gifted")]
    public bool? Gifted { get; init; }

    /// <summary>Gets the username of the person who gifted the subscription.</summary>
    [JsonPropertyName("sender")]
    public string? Sender { get; init; }

    /// <summary>Gets the user avatar URL.</summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; init; }

    /// <summary>Gets the provider-specific user ID.</summary>
    [JsonPropertyName("providerId")]
    public string? ProviderId { get; init; }

    /// <summary>Gets the StreamElements tip ID for tip events.</summary>
    [JsonPropertyName("tipId")]
    public string? TipId { get; init; }
}
