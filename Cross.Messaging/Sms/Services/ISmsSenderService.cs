namespace Cross.Messaging.Sms.Services;

/// <summary>
/// Sends SMS notifications.
/// </summary>
public interface ISmsSenderService
{
    /// <summary>
    /// Sends an SMS message to the specified destination.
    /// </summary>
    /// <param name="destination">Recipient phone number or destination address.</param>
    /// <param name="body">SMS message body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Provider-specific message identifier.</returns>
    Task<string> SendAsync(string destination, string body, CancellationToken cancellationToken);
}
