namespace StreamElements.Client.Options;

/// <summary>
/// Specifies the authentication method used when connecting to the StreamElements realtime endpoint.
/// </summary>
public enum StreamElementsAuthMethod
{
    /// <summary>JWT personal access token from the StreamElements dashboard.</summary>
    Jwt,

    /// <summary>OAuth2 access token obtained via the authorization code flow.</summary>
    OAuth2,
}
