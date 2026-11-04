namespace Cross.Messaging.Sms.Providers;

/// <summary>
/// Sends SMS messages through a concrete provider backend.
/// </summary>
public interface ISmsProvider
{
    /// <summary>
    /// Provider name used for configuration lookup (for example <c>Telnyx</c>).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Sends an SMS message using the given provider settings.
    /// </summary>
    /// <param name="destination">Recipient phone number in E.164 format.</param>
    /// <param name="body">SMS message body.</param>
    /// <param name="options">Provider-specific settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Provider-specific message identifier.</returns>
    Task<string> SendAsync(
        string destination,
        string body,
        MessagingSmsProviderOptions options,
        CancellationToken cancellationToken);
}
