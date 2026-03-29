# StreamElements.Client

Typed .NET 10 client library for the StreamElements REST API and real-time socket events.

## Features

- **Realtime event client** — connects to StreamElements Socket.IO v2 endpoint over `System.Net.WebSockets` (no third-party socket.io dependency)
- **Typed events** — `tip`, `subscriber`, `cheer`, `follow`, `host`, `raid` with strongly-typed payloads
- **Auto-reconnect** — linear backoff with configurable retry
- **JWT and OAuth2 auth** — supports both StreamElements authentication methods
- **DI integration** — `AddStreamElementsClient()` extension for `IServiceCollection`
- **HTTP client** — typed `StreamElementsHttpClient` for REST API calls

## Quick Start

```csharp
using Microsoft.Extensions.DependencyInjection;
using StreamElements.Client.Abstractions;
using StreamElements.Client.DependencyInjection;
using StreamElements.Client.Events;
using StreamElements.Client.Options;

var services = new ServiceCollection();
services.AddLogging();
services.AddStreamElementsClient(opts =>
{
    opts.Token = "your-jwt-token-here";
    opts.AuthMethod = StreamElementsAuthMethod.Jwt;
});

await using var sp = services.BuildServiceProvider();
var client = sp.GetRequiredService<IStreamElementsRealtimeClient>();

client.Authenticated += (_, channelId) =>
    Console.WriteLine($"Authenticated as channel: {channelId}");

client.EventReceived += (_, evt) =>
{
    if (evt is StreamElementsTipEvent tip)
        Console.WriteLine($"Tip: {tip.Data?.Amount} {tip.Data?.Currency} from {tip.Data?.Username}");
};

using var cts = new CancellationTokenSource();
await client.RunAsync(cts.Token);
```

## Packages

| Package | Description |
|---------|-------------|
| `StreamElements.Client` | Core realtime client and typed events |
| `StreamElements.Client.DependencyInjection` | `IServiceCollection` extensions |

## REST API Client (Kiota)

A Kiota-generated REST client can be generated using `scripts/regen-kiota.sh`. See the script for details.

## License

MIT — see [LICENSE.txt](LICENSE.txt).
