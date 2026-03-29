namespace StreamElements.Client.Events;

/// <summary>
/// Well-known StreamElements realtime event type strings.
/// </summary>
public static class StreamElementsEventType
{
    /// <summary>Bits cheer event from Twitch.</summary>
    public const string Cheer = "cheer";

    /// <summary>New follower event.</summary>
    public const string Follow = "follow";

    /// <summary>Channel host event.</summary>
    public const string Host = "host";

    /// <summary>Raid event.</summary>
    public const string Raid = "raid";

    /// <summary>New subscriber or resub event.</summary>
    public const string Subscriber = "subscriber";

    /// <summary>Tip (donation) event.</summary>
    public const string Tip = "tip";
}
