namespace Cross.Messaging.Email.Options;

/// <summary>
/// Configuration options for email sender services.
/// </summary>
public class MessagingEmailOptions
{
    /// <summary>
    /// Configuration section name for binding email settings.
    /// </summary>
    public const string SectionName = "MessagingEmail";

    /// <summary>
    /// SMTP server host name.
    /// </summary>
    public string SmtpHost { get; set; }

    /// <summary>
    /// SMTP server port.
    /// </summary>
    public int SmtpPort { get; set; }

    /// <summary>
    /// Enables SSL/TLS for <see cref="SmtpClient" /> flow.
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// 465 => SecureSocketOptions.SslOnConnect,
    /// 587 => SecureSocketOptions.StartTls,
    /// 25  => SecureSocketOptions.None,
    /// _   => SecureSocketOptions.Auto
    /// </summary>
    public SecureSocketOptions SecureSocket { get; set; }

    /// <summary>
    /// SMTP account login.
    /// </summary>
    public string SmtpLogin { get; set; }

    /// <summary>
    /// SMTP account password.
    /// </summary>
    public string SmtpPassword { get; set; }

    /// <summary>
    /// Sender display name.
    /// </summary>
    public string FromUserName { get; set; }

    /// <summary>
    /// Sender email address.
    /// </summary>
    public string FromUserAddress { get; set; }

    /// <summary>
    /// Optional recipient override used to redirect all outgoing emails.
    /// </summary>
    public string RecipientOverride { get; set; }
}
