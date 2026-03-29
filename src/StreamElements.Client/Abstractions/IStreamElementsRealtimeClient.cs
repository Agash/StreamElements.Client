using StreamElements.Client.Events;

namespace StreamElements.Client.Abstractions;

/// <summary>
/// Defines a client that connects to the StreamElements realtime socket endpoint
/// and delivers typed event notifications.
/// </summary>
public interface IStreamElementsRealtimeClient : IAsyncDisposable
{
    /// <summary>
    /// Gets whether the client is currently connected and authenticated.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the channel ID returned by the server after authentication.
    /// Null until the first successful authentication.
    /// </summary>
    string? AuthenticatedChannelId { get; }

    /// <summary>
    /// Raised when a realtime event is received from StreamElements.
    /// </summary>
    event EventHandler<StreamElementsRealtimeEvent>? EventReceived;

    /// <summary>
    /// Raised when a session stat update is received from StreamElements.
    /// </summary>
    event EventHandler<StreamElementsSessionUpdateEvent>? SessionUpdateReceived;

    /// <summary>
    /// Raised when the connection authenticates successfully.
    /// </summary>
    event EventHandler<string>? Authenticated;

    /// <summary>
    /// Raised when the connection is closed (either gracefully or due to error).
    /// </summary>
    event EventHandler<Exception?>? Disconnected;

    /// <summary>
    /// Starts the connection and authentication loop. Runs until the cancellation token is cancelled.
    /// Automatically reconnects on transient failures.
    /// </summary>
    Task RunAsync(CancellationToken cancellationToken);
}
