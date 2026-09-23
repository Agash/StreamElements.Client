namespace StreamElements.Client.Internal.SocketIo;

/// <summary>Payload of the Socket.IO <c>authenticate</c> event.</summary>
/// <param name="Method"><c>jwt</c> or <c>oauth2</c>.</param>
/// <param name="Token">The JWT or OAuth2 access token.</param>
internal sealed record SocketIoAuthPayload(string Method, string Token);
