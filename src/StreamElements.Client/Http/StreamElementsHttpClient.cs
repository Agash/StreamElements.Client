namespace StreamElements.Client;

/// <summary>
/// Named HttpClient for StreamElements REST API calls.
/// Inject and use directly for typed API requests until a Kiota-generated client is available.
/// </summary>
public sealed class StreamElementsHttpClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="StreamElementsHttpClient"/>.
    /// </summary>
    public StreamElementsHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Gets the underlying <see cref="HttpClient"/>.</summary>
    public HttpClient HttpClient => _httpClient;
}
