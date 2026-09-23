using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StreamElements.Client.Abstractions;
using StreamElements.Client.Options;
using StreamElements.Client.Realtime;

namespace StreamElements.Client.DependencyInjection;

/// <summary>
/// Extension methods for registering StreamElements.Client services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers StreamElements client services with the specified options.
    /// </summary>
    public static IServiceCollection AddStreamElementsClient(
        this IServiceCollection services,
        Action<StreamElementsClientOptions> configureOptions
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.TryAddSingleton<IStreamElementsRealtimeClient, StreamElementsRealtimeClient>();
        services.AddHttpClient<StreamElementsHttpClient>(
            (sp, client) =>
            {
                StreamElementsClientOptions opts = sp.GetRequiredService<
                    IOptions<StreamElementsClientOptions>
                >().Value;
                client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
                if (!string.IsNullOrWhiteSpace(opts.Token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", opts.Token);
                }
            }
        );
        return services;
    }

    /// <summary>
    /// Registers StreamElements client services, reading options from configuration.
    /// </summary>
    public static IServiceCollection AddStreamElementsClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IStreamElementsRealtimeClient, StreamElementsRealtimeClient>();
        services.AddHttpClient<StreamElementsHttpClient>();
        return services;
    }
}
