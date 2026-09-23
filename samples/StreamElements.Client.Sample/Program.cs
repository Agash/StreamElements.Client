using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using StreamElements.Client.Abstractions;
using StreamElements.Client.DependencyInjection;
using StreamElements.Client.Events;
using StreamElements.Client.Options;

CancellationTokenSource shutdown = new();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    shutdown.Cancel();
};

try
{
    await SampleApplication.RunAsync(shutdown).ConfigureAwait(false);
}
catch (OperationCanceledException)
{
    // Normal shutdown.
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    Environment.ExitCode = 1;
}

internal static class SampleApplication
{
    public static async Task RunAsync(CancellationTokenSource shutdownSource)
    {
        CancellationToken cancellationToken = shutdownSource.Token;
        AnsiConsole.Clear();

        AnsiConsole.Write(new FigletText("StreamElements").Color(Color.SteelBlue1));

        AnsiConsole.MarkupLine(
            "[grey]StreamElements realtime event listener - JWT auth, zero third-party deps.[/]"
        );
        AnsiConsole.WriteLine();

        string token = AnsiConsole.Prompt(
            new TextPrompt<string>("StreamElements [green]JWT token[/]?")
                .PromptStyle("deepskyblue1")
                .Secret()
        );

        AnsiConsole.WriteLine();

        ConcurrentQueue<StreamElementsRealtimeEvent> receivedEvents = new();
        Dictionary<string, int> eventCounts = new(StringComparer.Ordinal);
        object consoleLock = new();

        ServiceCollection services = new();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning).AddConsole());
        services.AddStreamElementsClient(opts =>
        {
            opts.Token = token;
            opts.AuthMethod = StreamElementsAuthMethod.Jwt;
        });

        await using ServiceProvider sp = services.BuildServiceProvider();
        IStreamElementsRealtimeClient client =
            sp.GetRequiredService<IStreamElementsRealtimeClient>();

        client.Authenticated += (_, channelId) =>
        {
            lock (consoleLock)
            {
                AnsiConsole.MarkupLineInterpolated(
                    $"[green]Authenticated[/] - channel ID: [white]{Markup.Escape(channelId)}[/]"
                );
            }
        };

        client.Disconnected += (_, ex) =>
        {
            lock (consoleLock)
            {
                if (ex is null)
                    AnsiConsole.MarkupLine("[yellow]Disconnected.[/]");
                else
                    AnsiConsole.MarkupLineInterpolated(
                        $"[red]Disconnected:[/] {Markup.Escape(ex.Message)}"
                    );
            }
        };

        client.EventReceived += (_, evt) =>
        {
            receivedEvents.Enqueue(evt);
            lock (consoleLock)
            {
                eventCounts.TryGetValue(evt.Type, out int count);
                eventCounts[evt.Type] = count + 1;
                RenderEvent(evt);
            }
        };

        // Run the realtime client in the background
        Task clientTask = Task.Run(() => client.RunAsync(cancellationToken), cancellationToken);

        AnsiConsole.MarkupLine(
            "[grey]Connecting to StreamElements realtime... Press Ctrl+C to exit.[/]"
        );
        AnsiConsole.WriteLine();

        // Command loop
        while (!cancellationToken.IsCancellationRequested)
        {
            AnsiConsole.WriteLine();

            string command = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Choose an action[/]")
                    .AddChoices(
                        "Show recent events",
                        "Show event counts",
                        "Show connection status",
                        "Exit"
                    )
            );

            switch (command)
            {
                case "Show recent events":
                    lock (consoleLock)
                    {
                        if (receivedEvents.IsEmpty)
                        {
                            AnsiConsole.MarkupLine("[yellow]No events received yet.[/]");
                            break;
                        }

                        StreamElementsRealtimeEvent[] snapshot = [.. receivedEvents];
                        Table table = new Table()
                            .RoundedBorder()
                            .AddColumn("[bold]Type[/]")
                            .AddColumn("[bold]Username[/]")
                            .AddColumn("[bold]Amount[/]")
                            .AddColumn("[bold]Provider[/]")
                            .AddColumn("[bold]Created[/]");

                        foreach (StreamElementsRealtimeEvent e in snapshot.TakeLast(20))
                        {
                            string username = GetUsername(e);
                            string amount = GetAmount(e);
                            table.AddRow(
                                Markup.Escape(e.Type),
                                Markup.Escape(username),
                                Markup.Escape(amount),
                                Markup.Escape(e.Provider ?? "-"),
                                Markup.Escape(e.CreatedAt?.ToString("u") ?? "-")
                            );
                        }

                        AnsiConsole.Write(table);
                    }
                    break;

                case "Show event counts":
                    lock (consoleLock)
                    {
                        Table table = new Table()
                            .RoundedBorder()
                            .AddColumn("[bold]Event Type[/]")
                            .AddColumn("[bold]Count[/]");

                        foreach (
                            (string type, int count) in eventCounts.OrderByDescending(x => x.Value)
                        )
                        {
                            table.AddRow(Markup.Escape(type), count.ToString());
                        }

                        AnsiConsole.Write(table);
                    }
                    break;

                case "Show connection status":
                    lock (consoleLock)
                    {
                        string status = client.IsConnected
                            ? "[green]Connected[/]"
                            : "[red]Disconnected[/]";
                        string channelId = client.AuthenticatedChannelId is null
                            ? "[grey](not authenticated)[/]"
                            : $"[white]{Markup.Escape(client.AuthenticatedChannelId)}[/]";
                        AnsiConsole.MarkupLineInterpolated(
                            $"Status: {status} | Channel: {channelId}"
                        );
                    }
                    break;

                case "Exit":
                    shutdownSource.Cancel();
                    break;
            }

            await Task.Yield();
        }

        try
        {
            await clientTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
    }

    private static void RenderEvent(StreamElementsRealtimeEvent evt)
    {
        Grid grid = new();
        grid.AddColumn();
        grid.AddColumn();

        grid.AddRow("[bold]Type[/]", Markup.Escape(evt.Type));
        grid.AddRow("[bold]Provider[/]", Markup.Escape(evt.Provider ?? "-"));
        grid.AddRow("[bold]Username[/]", Markup.Escape(GetUsername(evt)));

        string amount = GetAmount(evt);
        if (amount != "-")
            grid.AddRow("[bold]Amount[/]", Markup.Escape(amount));

        string message = GetMessage(evt);
        if (!string.IsNullOrWhiteSpace(message))
            grid.AddRow("[bold]Message[/]", Markup.Escape(message));

        Color borderColor = evt.Type switch
        {
            "tip" => Color.Gold1,
            "subscriber" => Color.MediumPurple1,
            "cheer" => Color.SteelBlue1,
            "follow" => Color.Green,
            "raid" => Color.OrangeRed1,
            _ => Color.Grey,
        };

        AnsiConsole.Write(
            new Panel(grid)
                .Header($"[bold]{Markup.Escape(evt.Type.ToUpperInvariant())}[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(borderColor)
        );
    }

    private static string GetUsername(StreamElementsRealtimeEvent evt) =>
        evt switch
        {
            StreamElementsTipEvent e => e.Data?.DisplayName ?? e.Data?.Username ?? "-",
            StreamElementsSubscriberEvent e => e.Data?.DisplayName ?? e.Data?.Username ?? "-",
            StreamElementsCheerEvent e => e.Data?.DisplayName ?? e.Data?.Username ?? "-",
            StreamElementsFollowEvent e => e.Data?.DisplayName ?? e.Data?.Username ?? "-",
            StreamElementsHostEvent e => e.Data?.DisplayName ?? e.Data?.Username ?? "-",
            StreamElementsRaidEvent e => e.Data?.DisplayName ?? e.Data?.Username ?? "-",
            _ => "-",
        };

    private static string GetAmount(StreamElementsRealtimeEvent evt) =>
        evt switch
        {
            StreamElementsTipEvent e when e.Data?.Amount is { } a =>
                $"{a:0.00} {e.Data.Currency ?? ""}".Trim(),
            StreamElementsSubscriberEvent e when e.Data?.Amount is { } a => $"{a} months",
            StreamElementsCheerEvent e when e.Data?.Amount is { } a => $"{a} bits",
            StreamElementsRaidEvent e when e.Data?.Amount is { } a => $"{a} raiders",
            StreamElementsHostEvent e when e.Data?.Amount is { } a => $"{a} viewers",
            _ => "-",
        };

    private static string GetMessage(StreamElementsRealtimeEvent evt) =>
        evt switch
        {
            StreamElementsTipEvent e => e.Data?.Message ?? string.Empty,
            StreamElementsSubscriberEvent e => e.Data?.Message ?? string.Empty,
            StreamElementsCheerEvent e => e.Data?.Message ?? string.Empty,
            _ => string.Empty,
        };
}
