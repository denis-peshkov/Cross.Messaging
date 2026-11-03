namespace Cross.Messaging.Sms.Services;

/// <inheritdoc />
public class SmsSenderService : ISmsSenderService
{
    private readonly ILogger<SmsSenderService> _logger;
    private readonly MessagingSmsOptions _options;
    private readonly IReadOnlyDictionary<string, ISmsProvider> _providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmsSenderService" /> class.
    /// </summary>
    /// <param name="logger">Logger for send operation events and errors.</param>
    /// <param name="options">Snapshot with SMS sender configuration.</param>
    /// <param name="providers">Registered SMS providers.</param>
    public SmsSenderService(
        ILogger<SmsSenderService> logger,
        IOptionsSnapshot<MessagingSmsOptions> options,
        IEnumerable<ISmsProvider> providers)
    {
        _logger = logger;
        _options = options.Value;
        _providers = providers
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task<string> SendAsync(string destination, string body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new ArgumentException("Destination phone number is required.", nameof(destination));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Body is required.", nameof(body));
        }

        var providerName = ResolveProviderName(destination);
        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new InvalidOperationException(
                "SMS provider is not configured. Set MessagingSms:DefaultProvider or CountryProviders.");
        }

        if (!_providers.TryGetValue(providerName, out var provider))
        {
            throw new InvalidOperationException($"SMS provider '{providerName}' is not registered.");
        }

        if (!TryGetProviderOptions(providerName, out var providerOptions))
        {
            throw new InvalidOperationException(
                $"SMS provider settings for '{providerName}' are missing under MessagingSms:Providers.");
        }

        var maskedDestination = PhoneDestinationMask.Mask(destination);
        _logger.LogInformation(
            "Sending SMS via {Provider} to {Destination}.",
            provider.Name,
            maskedDestination);

        try
        {
            var messageId = await provider.SendAsync(destination, body, providerOptions, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "SMS sent via {Provider}. MessageId={MessageId}.",
                provider.Name,
                messageId);

            return messageId;
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not ArgumentException)
        {
            _logger.LogError(ex, "Failed to send SMS via {Provider} to {Destination}.", provider.Name, maskedDestination);
            if (ex is InvalidOperationException)
            {
                throw;
            }

            throw new InvalidOperationException($"Failed to send SMS via '{provider.Name}'.", ex);
        }
    }

    private string ResolveProviderName(string destination)
    {
        if (PhoneCountryResolver.TryResolve(destination, out var country) &&
            TryGetCountryProvider(country, out var mapped))
        {
            return mapped;
        }

        return _options.DefaultProvider;
    }

    private bool TryGetCountryProvider(string country, out string providerName)
    {
        providerName = null!;
        if (_options.CountryProviders is null)
        {
            return false;
        }

        foreach (var pair in _options.CountryProviders)
        {
            if (string.Equals(pair.Key, country, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(pair.Value))
            {
                providerName = pair.Value;
                return true;
            }
        }

        return false;
    }

    private bool TryGetProviderOptions(string providerName, out MessagingSmsProviderOptions providerOptions)
    {
        providerOptions = null!;
        if (_options.Providers is null)
        {
            return false;
        }

        foreach (var pair in _options.Providers)
        {
            if (string.Equals(pair.Key, providerName, StringComparison.OrdinalIgnoreCase) &&
                pair.Value is not null)
            {
                providerOptions = pair.Value;
                return true;
            }
        }

        return false;
    }
}
