namespace Cross.Messaging.Email.Services;

/// <summary>
/// Shared try / catch / log for SMTP send operations (System.Net.Mail and MailKit).
/// </summary>
internal static class EmailSendLogging
{
    /// <summary>
    /// Executes the send operation and writes success/error logs with recipient context.
    /// </summary>
    /// <param name="send">Asynchronous send operation.</param>
    /// <param name="recipientEmail">Recipient email used for log context.</param>
    /// <param name="logger">Logger instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="SmtpException">Thrown when SMTP-specific errors occur.</exception>
    internal static async Task RunSendAndLogAsync(
        Func<Task> send,
        string recipientEmail,
        ILogger<EmailSenderService> logger)
    {
        try
        {
            await send();
            logger.LogInformation("Email sent to {Recipient}", recipientEmail);
        }
        catch (SmtpException ex)
        {
            logger.LogError(ex, "SMTP error when sending email to {Recipient}", recipientEmail);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error when sending email to {Recipient}", recipientEmail);
            throw;
        }
    }
}
