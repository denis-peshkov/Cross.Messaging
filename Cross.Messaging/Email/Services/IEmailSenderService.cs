namespace Cross.Messaging.Email.Services;

/// <summary>
/// Sends email notifications using configured SMTP transport.
/// </summary>
public interface IEmailSenderService
{
    /// <summary>
    /// Sends an HTML email message.
    /// </summary>
    /// <param name="toName">Recipient display name.</param>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="subject">Message subject.</param>
    /// <param name="body">HTML message body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    Task SendAsync(string toName, string toEmail, string subject, string body, CancellationToken cancellationToken);

    /// <summary>
    /// Sends an email with plain-text and HTML alternatives.
    /// </summary>
    /// <param name="toName">Recipient display name.</param>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="subject">Message subject.</param>
    /// <param name="textBody">Plain-text message body.</param>
    /// <param name="htmlBody">HTML message body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    Task SendAsync(string toName, string toEmail, string subject, string textBody, string htmlBody, CancellationToken cancellationToken);
}
