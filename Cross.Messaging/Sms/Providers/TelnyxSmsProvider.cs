namespace Cross.Messaging.Sms.Providers;

/// <summary>
/// Telnyx Messages API SMS provider.
/// </summary>
public sealed class TelnyxSmsProvider : ISmsProvider
{
    /// <inheritdoc />
    public string Name => "Telnyx";

    /// <inheritdoc />
    public async Task<string> SendAsync(
        string destination,
        string body,
        MessagingSmsProviderOptions options,
        CancellationToken cancellationToken)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException("Telnyx ApiKey is required.");
        }

        if (string.IsNullOrWhiteSpace(options.From) && string.IsNullOrWhiteSpace(options.MessagingProfileId))
        {
            throw new InvalidOperationException("Telnyx From or MessagingProfileId is required.");
        }

        var payload = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["to"] = destination,
            ["text"] = body,
        };

        if (!string.IsNullOrWhiteSpace(options.From))
        {
            payload["from"] = options.From;
        }

        if (!string.IsNullOrWhiteSpace(options.MessagingProfileId))
        {
            payload["messaging_profile_id"] = options.MessagingProfileId;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "v2/messages")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Telnyx SMS send failed with status {(int)response.StatusCode} ({response.StatusCode}). Response: {Truncate(responseBody)}");
        }

        TelnyxMessageResponse? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<TelnyxMessageResponse>(responseBody, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Telnyx SMS send succeeded but response JSON is invalid.", ex);
        }

        var id = parsed?.Data?.Id;
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException("Telnyx SMS send succeeded but message id is missing.");
        }

        return id!;
    }

    private const int MaxErrorResponseLength = 512;

    private static string Truncate(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Length <= MaxErrorResponseLength
            ? value
            : value.Substring(0, MaxErrorResponseLength) + "...";
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelnyxSmsProvider" /> class.
    /// </summary>
    /// <param name="httpClient">HTTP client used to call Telnyx API.</param>
    public TelnyxSmsProvider(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri("https://api.telnyx.com/");
        }
    }

    private sealed class TelnyxMessageResponse
    {
        public TelnyxMessageData? Data { get; set; }
    }

    private sealed class TelnyxMessageData
    {
        public string? Id { get; set; }
    }
}
