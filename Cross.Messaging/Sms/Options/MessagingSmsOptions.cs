namespace Cross.Messaging.Sms.Options;

/// <summary>
/// Configuration options for SMS sender services.
/// </summary>
public class MessagingSmsOptions
{
    /// <summary>
    /// Configuration section name for binding SMS settings.
    /// </summary>
    public const string SectionName = "MessagingSms";

    /// <summary>
    /// Provider name used when the destination country is missing from <see cref="CountryProviders" />.
    /// </summary>
    public string DefaultProvider { get; set; }

    /// <summary>
    /// Maps ISO country codes (for example <c>US</c>, <c>UA</c>) to SMS provider names.
    /// </summary>
    public Dictionary<string, string> CountryProviders { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Provider-specific settings keyed by provider name (for example <c>Telnyx</c>).
    /// </summary>
    public Dictionary<string, MessagingSmsProviderOptions> Providers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Settings for a single SMS provider instance.
/// </summary>
public class MessagingSmsProviderOptions
{
    /// <summary>
    /// API key / bearer token used to authenticate with the provider.
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// Sender address in E.164 format, short code, or alphanumeric sender ID.
    /// </summary>
    public string From { get; set; }

    /// <summary>
    /// Optional messaging profile identifier (required by some Telnyx send modes).
    /// </summary>
    public string MessagingProfileId { get; set; }
}
