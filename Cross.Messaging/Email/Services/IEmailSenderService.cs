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
    /// Sends an HTML email message.
    /// </summary>
    /// <param name="toName">Recipient display name.</param>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="subject">Message subject.</param>
    /// <param name="body">HTML message body.</param>
    /// <param name="priority">Message priority header value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    Task SendAsync(string toName, string toEmail, string subject, string body, MailPriority priority, CancellationToken cancellationToken);

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

    /// <summary>
    /// Sends an email with plain-text and HTML alternatives.
    /// </summary>
    /// <param name="toName">Recipient display name.</param>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="subject">Message subject.</param>
    /// <param name="textBody">Plain-text message body.</param>
    /// <param name="htmlBody">HTML message body.</param>
    /// <param name="attachments">Optional collection of file attachments.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    Task SendAsync(string toName, string toEmail, string subject, string textBody, string htmlBody, IReadOnlyCollection<IFormFile>? attachments, CancellationToken cancellationToken);

    /// <summary>
    /// Sends an email with plain-text and HTML alternatives.
    /// </summary>
    /// <param name="toName">Recipient display name.</param>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="subject">Message subject.</param>
    /// <param name="textBody">Plain-text message body.</param>
    /// <param name="htmlBody">HTML message body.</param>
    /// <param name="priority">Message priority header value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    Task SendAsync(string toName, string toEmail, string subject, string textBody, string htmlBody, XMessagePriority priority, CancellationToken cancellationToken);

    /// <summary>
    /// Sends an email with plain-text and HTML alternatives.
    /// </summary>
    /// <param name="toName">Recipient display name.</param>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="subject">Message subject.</param>
    /// <param name="textBody">Plain-text message body.</param>
    /// <param name="htmlBody">HTML message body.</param>
    /// <param name="priority">Message priority header value.</param>
    /// <param name="attachments">Optional collection of file attachments.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    Task SendAsync(string toName, string toEmail, string subject, string textBody, string htmlBody, XMessagePriority priority, IReadOnlyCollection<IFormFile> attachments, CancellationToken cancellationToken);

    /// <summary>
    /// Sends an email with optional attachments and assigns custom content identifiers.
    /// </summary>
    /// <param name="toName">Recipient display name.</param>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="subject">Message subject.</param>
    /// <param name="textBody">Plain-text message body.</param>
    /// <param name="htmlBody">HTML message body.</param>
    /// <param name="priority">Message priority header value.</param>
    /// <param name="attachments">Optional collection of attached files.</param>
    /// <param name="contentIdMap">Optional map of content identifiers to attachment file names.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A dictionary that contains the effective mapping between content identifiers and file names
    /// for attachments included in the sent message.
    /// </returns>
    Task<Dictionary<string, string>> SendAsyncWithContentIds(string toName, string toEmail, string subject, string textBody, string htmlBody, XMessagePriority priority, IReadOnlyCollection<IFormFile> attachments, IReadOnlyCollection<KeyValuePair<string, string>> contentIdMap, CancellationToken cancellationToken);

    /// <summary>
    /// Sends an email with optional attachments and assigns custom content identifiers.
    /// </summary>
    /// <param name="toName">Recipient display name.</param>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="subject">Message subject.</param>
    /// <param name="textBody">Plain-text message body.</param>
    /// <param name="htmlBody">HTML message body.</param>
    /// <param name="attachments">Optional collection of attached files.</param>
    /// <param name="contentIdMap">Optional map of content identifiers to attachment file names.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A dictionary that contains the effective mapping between content identifiers and file names
    /// for attachments included in the sent message.
    /// </returns>
    Task<Dictionary<string, string>> SendAsyncWithContentIds(string toName, string toEmail, string subject, string textBody, string htmlBody, IReadOnlyCollection<IFormFile> attachments, IReadOnlyCollection<KeyValuePair<string, string>> contentIdMap, CancellationToken cancellationToken);
}
