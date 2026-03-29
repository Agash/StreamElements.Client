namespace StreamElements.Client.Options;

/// <summary>
/// Configuration options for the StreamElements client.
/// </summary>
public sealed class StreamElementsClientOptions
{
    /// <summary>
    /// Gets or sets the StreamElements JWT personal access token or OAuth2 access token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authentication method used when connecting to the realtime endpoint.
    /// </summary>
    public StreamElementsAuthMethod AuthMethod { get; set; } = StreamElementsAuthMethod.Jwt;

    /// <summary>
    /// Gets or sets the REST API base URL.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.streamelements.com/kappa/v2";

    /// <summary>
    /// Gets or sets the realtime socket endpoint URL.
    /// </summary>
    public string RealtimeUrl { get; set; } = "wss://realtime.streamelements.com/socket.io/?transport=websocket&EIO=3";
}
