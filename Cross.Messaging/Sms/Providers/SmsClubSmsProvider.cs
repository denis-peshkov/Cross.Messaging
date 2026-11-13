namespace Cross.Messaging.Sms.Providers;

/// <summary>
/// SMSClub JSON API SMS provider.
/// </summary>
public sealed class SmsClubSmsProvider : ISmsProvider
{
    /// <inheritdoc />
    public string Name => "SMSClub";

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
            throw new InvalidOperationException("SMSClub ApiKey is required.");
        }

        if (string.IsNullOrWhiteSpace(options.From))
        {
            throw new InvalidOperationException("SMSClub From (src_addr) is required.");
        }

        var payload = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["phone"] = new[] { NormalizePhone(destination) },
            ["message"] = body,
            ["src_addr"] = options.From,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "sms/send")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"SMSClub SMS send failed with status {(int)response.StatusCode} ({response.StatusCode}). Response: {Truncate(responseBody)}");
        }

        SmsClubSendResponse? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<SmsClubSendResponse>(responseBody, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("SMSClub SMS send succeeded but response JSON is invalid.", ex);
        }

        var info = parsed?.SuccessRequest?.Info;
        if (info is not null && info.Count > 0)
        {
            return info.Keys.First();
        }

        var addInfo = parsed?.SuccessRequest?.AddInfo;
        if (addInfo is not null && addInfo.Count > 0)
        {
            throw new InvalidOperationException(
                $"SMSClub SMS send failed: {Truncate(FormatAddInfo(addInfo))}");
        }

        throw new InvalidOperationException("SMSClub SMS send succeeded but message id is missing.");
    }

    private static string FormatAddInfo(IReadOnlyDictionary<string, string> addInfo)
    {
        return string.Join("; ", addInfo.Select(pair => $"{PhoneDestinationMask.MaskIfPhone(pair.Key)}: {pair.Value}"));
    }

    private const int MaxErrorResponseLength = 512;

    private static string NormalizePhone(string destination)
    {
        var trimmed = destination.Trim();
        return trimmed.StartsWith("+", StringComparison.Ordinal) ? trimmed.Substring(1) : trimmed;
    }

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
    /// Initializes a new instance of the <see cref="SmsClubSmsProvider" /> class.
    /// </summary>
    /// <param name="httpClient">HTTP client used to call SMSClub API.</param>
    public SmsClubSmsProvider(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri("https://im.smsclub.mobi/");
        }
    }

    private sealed class SmsClubSendResponse
    {
        [JsonPropertyName("success_request")]
        public SmsClubSuccessRequest? SuccessRequest { get; set; }
    }

    private sealed class SmsClubSuccessRequest
    {
        [JsonPropertyName("info")]
        public Dictionary<string, string>? Info { get; set; }

        [JsonPropertyName("add_info")]
        public Dictionary<string, string>? AddInfo { get; set; }
    }
}
