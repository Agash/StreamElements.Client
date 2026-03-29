using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StreamElements.Client.Abstractions;
using StreamElements.Client.Events;
using StreamElements.Client.Internal.SocketIo;
using StreamElements.Client.Options;

namespace StreamElements.Client.Realtime;

/// <summary>
/// Connects to the StreamElements realtime socket endpoint using Engine.IO v3 / Socket.IO v2
/// framing over <see cref="ClientWebSocket"/>. No third-party socket.io library is required.
/// </summary>
public sealed partial class StreamElementsRealtimeClient : IStreamElementsRealtimeClient
{
    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web);

    private readonly StreamElementsClientOptions _options;
    private readonly ILogger<StreamElementsRealtimeClient> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="StreamElementsRealtimeClient"/>.
    /// </summary>
    public StreamElementsRealtimeClient(
        IOptions<StreamElementsClientOptions> options,
        ILogger<StreamElementsRealtimeClient> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsConnected { get; private set; }

    /// <inheritdoc />
    public string? AuthenticatedChannelId { get; private set; }

    /// <inheritdoc />
    public event EventHandler<StreamElementsRealtimeEvent>? EventReceived;

    /// <inheritdoc />
    public event EventHandler<StreamElementsSessionUpdateEvent>? SessionUpdateReceived;

    /// <inheritdoc />
    public event EventHandler<string>? Authenticated;

    /// <inheritdoc />
    public event EventHandler<Exception?>? Disconnected;

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        int attempt = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            attempt++;
            try
            {
                await ConnectAndRunAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                LogConnectionError(_logger, attempt, ex);
                IsConnected = false;
                Disconnected?.Invoke(this, ex);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                // Linear backoff: min 2s, max 30s
                int delaySeconds = Math.Min(2 * attempt, 30);
                LogReconnectDelay(_logger, delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken).ConfigureAwait(false);
            }
        }

        IsConnected = false;
        Disconnected?.Invoke(this, null);
    }

    private async Task ConnectAndRunAsync(CancellationToken cancellationToken)
    {
        using ClientWebSocket ws = new();
        ws.Options.SetRequestHeader("User-Agent", "StreamElements.Client/1.0 (.NET)");

        Uri uri = new(_options.RealtimeUrl);
        LogConnecting(_logger, uri);

        await ws.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
        LogConnected(_logger, uri);

        // Receive and process the Engine.IO open packet.
        string? openFrame = await ReceiveTextFrameAsync(ws, cancellationToken).ConfigureAwait(false);
        if (openFrame is null || !openFrame.StartsWith("0", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected Engine.IO open packet, got: {openFrame}");
        }
        LogEngineIoOpen(_logger, openFrame);

        // Receive the Socket.IO connect confirmation for the default namespace.
        string? connectFrame = await ReceiveTextFrameAsync(ws, cancellationToken).ConfigureAwait(false);
        if (connectFrame is null || !connectFrame.StartsWith("40", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected Socket.IO connect packet, got: {connectFrame}");
        }

        // Authenticate
        string authMethod = _options.AuthMethod == StreamElementsAuthMethod.OAuth2 ? "oauth2" : "jwt";
        var authPayload = new { method = authMethod, token = _options.Token };
        string authFrame = SocketIoFramer.EncodeEvent("authenticate", authPayload);
        await SendTextFrameAsync(ws, authFrame, cancellationToken).ConfigureAwait(false);
        LogAuthenticating(_logger, authMethod);

        // Event loop
        using CancellationTokenSource pingTimeoutCts = new(TimeSpan.FromSeconds(70));
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, pingTimeoutCts.Token);

        while (!linkedCts.Token.IsCancellationRequested
               && ws.State == WebSocketState.Open)
        {
            string? frame = await ReceiveTextFrameAsync(ws, linkedCts.Token).ConfigureAwait(false);
            if (frame is null)
            {
                break;
            }

            // EIO ping from server (text "2") — respond with pong (text "3")
            if (frame == "2")
            {
                pingTimeoutCts.CancelAfter(TimeSpan.FromSeconds(70)); // reset timeout
                await SendTextFrameAsync(ws, SocketIoFramer.Pong, linkedCts.Token).ConfigureAwait(false);
                LogPong(_logger);
                continue;
            }

            SocketIoPacket? packet = SocketIoFramer.TryParse(frame);
            if (packet is null)
            {
                continue;
            }

            HandlePacket(packet);
        }

        cancellationToken.ThrowIfCancellationRequested();
        pingTimeoutCts.Token.ThrowIfCancellationRequested();
    }

    private void HandlePacket(SocketIoPacket packet)
    {
        if (packet.Type == SocketIoPacketType.Disconnect)
        {
            LogServerDisconnect(_logger);
            IsConnected = false;
            Disconnected?.Invoke(this, null);
            return;
        }

        if (packet.Type != SocketIoPacketType.Event || string.IsNullOrEmpty(packet.Data))
        {
            return;
        }

        // packet.Data is a JSON array: ["eventName", payload]
        // or sometimes ["eventName"] with no payload.
        JsonElement[] parts;
        try
        {
            parts = JsonSerializer.Deserialize<JsonElement[]>(packet.Data, s_jsonOptions) ?? [];
        }
        catch (JsonException ex)
        {
            LogEventParseError(_logger, packet.Data, ex);
            return;
        }

        if (parts.Length == 0 || parts[0].ValueKind != JsonValueKind.String)
        {
            return;
        }

        string eventName = parts[0].GetString()!;
        JsonElement payload = parts.Length > 1 ? parts[1] : default;

        switch (eventName)
        {
            case "authenticated":
                HandleAuthenticated(payload);
                break;

            case "unauthorized":
                LogUnauthorized(_logger, payload.ToString());
                IsConnected = false;
                break;

            case "event":
                HandleEvent(payload);
                break;

            case "event:update":
                HandleSessionUpdate(payload);
                break;

            case "event:test":
                HandleEvent(payload); // treat test events the same as real ones
                break;

            default:
                LogUnknownSocketEvent(_logger, eventName);
                break;
        }
    }

    private void HandleAuthenticated(JsonElement payload)
    {
        IsConnected = true;
        string? channelId = null;
        if (payload.ValueKind == JsonValueKind.Object
            && payload.TryGetProperty("channelId", out JsonElement channelProp))
        {
            channelId = channelProp.GetString();
        }

        AuthenticatedChannelId = channelId;
        LogAuthenticated(_logger, channelId ?? "(unknown)");
        Authenticated?.Invoke(this, channelId ?? string.Empty);
    }

    private void HandleEvent(JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        string? type = null;
        if (payload.TryGetProperty("type", out JsonElement typeProp))
        {
            type = typeProp.GetString();
        }

        StreamElementsRealtimeEvent? evt = type switch
        {
            StreamElementsEventType.Tip => Deserialize<StreamElementsTipEvent>(payload),
            StreamElementsEventType.Subscriber => Deserialize<StreamElementsSubscriberEvent>(payload),
            StreamElementsEventType.Cheer => Deserialize<StreamElementsCheerEvent>(payload),
            StreamElementsEventType.Follow => Deserialize<StreamElementsFollowEvent>(payload),
            StreamElementsEventType.Host => Deserialize<StreamElementsHostEvent>(payload),
            StreamElementsEventType.Raid => Deserialize<StreamElementsRaidEvent>(payload),
            _ => Deserialize<StreamElementsUnknownEvent>(payload),
        };

        if (evt is not null)
        {
            EventReceived?.Invoke(this, evt);
        }
    }

    private void HandleSessionUpdate(JsonElement payload)
    {
        StreamElementsSessionUpdateEvent update = new() { Data = payload.Clone() };
        SessionUpdateReceived?.Invoke(this, update);
    }

    private static T? Deserialize<T>(JsonElement element)
    {
        try
        {
            return element.Deserialize<T>(s_jsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static async Task<string?> ReceiveTextFrameAsync(
        ClientWebSocket ws,
        CancellationToken cancellationToken)
    {
        using MemoryStream buffer = new();
        Memory<byte> chunk = new byte[4096];

        while (true)
        {
            ValueWebSocketReceiveResult result = await ws
                .ReceiveAsync(chunk, cancellationToken)
                .ConfigureAwait(false);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return null;
            }

            if (result.MessageType != WebSocketMessageType.Text)
            {
                // Skip binary frames (shouldn't occur in EIO v3 text mode).
                if (result.EndOfMessage) break;
                continue;
            }

            await buffer.WriteAsync(chunk[..result.Count], cancellationToken).ConfigureAwait(false);

            if (result.EndOfMessage)
            {
                break;
            }
        }

        return buffer.Length == 0 ? null : Encoding.UTF8.GetString(buffer.ToArray());
    }

    private static async Task SendTextFrameAsync(
        ClientWebSocket ws,
        string text,
        CancellationToken cancellationToken)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        await ws.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    // --- Log methods ---
    [LoggerMessage(EventId = 9001, Level = LogLevel.Information, Message = "Connecting to StreamElements realtime endpoint: {Uri}")]
    private static partial void LogConnecting(ILogger logger, Uri uri);

    [LoggerMessage(EventId = 9002, Level = LogLevel.Information, Message = "Connected to StreamElements realtime endpoint: {Uri}")]
    private static partial void LogConnected(ILogger logger, Uri uri);

    [LoggerMessage(EventId = 9003, Level = LogLevel.Debug, Message = "Engine.IO open: {Frame}")]
    private static partial void LogEngineIoOpen(ILogger logger, string frame);

    [LoggerMessage(EventId = 9004, Level = LogLevel.Information, Message = "Authenticating with method '{Method}'")]
    private static partial void LogAuthenticating(ILogger logger, string method);

    [LoggerMessage(EventId = 9005, Level = LogLevel.Information, Message = "Authenticated — channelId: {ChannelId}")]
    private static partial void LogAuthenticated(ILogger logger, string channelId);

    [LoggerMessage(EventId = 9006, Level = LogLevel.Warning, Message = "Authentication rejected: {Reason}")]
    private static partial void LogUnauthorized(ILogger logger, string reason);

    [LoggerMessage(EventId = 9007, Level = LogLevel.Debug, Message = "Received EIO ping, sent pong")]
    private static partial void LogPong(ILogger logger);

    [LoggerMessage(EventId = 9008, Level = LogLevel.Warning, Message = "Connection attempt {Attempt} failed")]
    private static partial void LogConnectionError(ILogger logger, int attempt, Exception ex);

    [LoggerMessage(EventId = 9009, Level = LogLevel.Information, Message = "Reconnecting in {Seconds}s")]
    private static partial void LogReconnectDelay(ILogger logger, int seconds);

    [LoggerMessage(EventId = 9010, Level = LogLevel.Information, Message = "Server closed the Socket.IO connection")]
    private static partial void LogServerDisconnect(ILogger logger);

    [LoggerMessage(EventId = 9011, Level = LogLevel.Debug, Message = "Received unknown socket.io event: {EventName}")]
    private static partial void LogUnknownSocketEvent(ILogger logger, string eventName);

    [LoggerMessage(EventId = 9012, Level = LogLevel.Warning, Message = "Failed to parse event payload: {Payload}")]
    private static partial void LogEventParseError(ILogger logger, string payload, Exception ex);
}
