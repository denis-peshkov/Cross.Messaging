namespace Cross.Messaging.Email.Services;

/// <summary>
/// Shared try / catch / log for SMTP send operations (System.Net.Mail and MailKit).
/// </summary>
internal static class EmailSendLogging
{
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
