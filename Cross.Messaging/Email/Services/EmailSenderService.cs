namespace Cross.Messaging.Email.Services;

/// <summary>
/// Default SMTP-based implementation of <see cref="IEmailSenderService" />.
/// </summary>
public class EmailSenderService : IEmailSenderService
{
    private readonly ILogger<EmailSenderService> _logger;
    private readonly MessagingEmailOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSenderService" /> class.
    /// </summary>
    /// <param name="logger">Logger for send operation events and errors.</param>
    /// <param name="options">Snapshot with email sender configuration.</param>
    public EmailSenderService(
        ILogger<EmailSenderService> logger,
        IOptionsSnapshot<MessagingEmailOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task SendAsync(string toName, string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException("Recipient email is required.", nameof(toEmail));
        }
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Subject is required.", nameof(subject));
        }
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Body is required.", nameof(body));
        }

        if (!string.IsNullOrWhiteSpace(_options.RecipientOverride))
        {
            toEmail = _options.RecipientOverride;
        }

        var fromAddress = new MailAddress(_options.FromUserAddress, _options.FromUserName);
        var toAddress = string.IsNullOrEmpty(toName)
            ? new MailAddress(toEmail)
            : new MailAddress(toEmail, toName);

        using var smtp = new SmtpClient
        {
            Host = _options.SmtpHost,
            Port = _options.SmtpPort,
            EnableSsl = _options.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(_options.SmtpLogin, _options.SmtpPassword),
            UseDefaultCredentials = false,
        };

        using var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };

        await EmailSendLogging.RunSendAndLogAsync(
            async () =>
            {
#if NETSTANDARD2_1
                await smtp.SendMailAsync(message);
#else
                await smtp.SendMailAsync(message, cancellationToken);
#endif
            },
            toEmail,
            _logger);
    }

    /// <inheritdoc />
    public async Task SendAsync(string toName, string toEmail, string subject, string textBody, string htmlBody, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException("Recipient email is required.", nameof(toEmail));
        }
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Subject is required.", nameof(subject));
        }
        if (string.IsNullOrWhiteSpace(textBody))
        {
            throw new ArgumentException("Text body is required.", nameof(textBody));
        }
        if (string.IsNullOrWhiteSpace(htmlBody))
        {
            throw new ArgumentException("HTML body is required.", nameof(htmlBody));
        }

        if (!string.IsNullOrWhiteSpace(_options.RecipientOverride))
        {
            toEmail = _options.RecipientOverride;
        }

        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromUserName, _options.FromUserAddress));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        // message.ReplyTo.Add(new MailboxAddress(_options.FromUserName, _options.FromUserAddress));

        var builder = new BodyBuilder
        {
            TextBody = textBody,
            HtmlBody = htmlBody,
        };

        // Note: mail clients choose whether to show HTML or plain text.
        message.Body = builder.ToMessageBody();

        using var smtp = new MailKit.Net.Smtp.SmtpClient();
        await smtp.ConnectAsync(_options.SmtpHost, _options.SmtpPort, _options.SecureSocket, cancellationToken);
        await smtp.AuthenticateAsync(_options.SmtpLogin, _options.SmtpPassword, cancellationToken);

        try
        {
            await EmailSendLogging.RunSendAndLogAsync(
                () => smtp.SendAsync(message, cancellationToken),
                toEmail,
                _logger);
        }
        finally
        {
            await smtp.DisconnectAsync(true, cancellationToken);
        }
    }

    public async Task SendAsync(string toName, string toEmail, string subject, string textBody, string htmlBody, IEnumerable<IFormFile>? attachments, CancellationToken cancellationToken)
    {
         if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException("Recipient email is required.", nameof(toEmail));
        }

        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromUserName, _options.FromUserAddress));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        // message.ReplyTo.Add(new MailboxAddress(_options.FromUserName, _options.FromUserAddress));

        var builder = new BodyBuilder
        {
            TextBody = textBody,
            HtmlBody = htmlBody,
        };

        // Добавляем вложения, если они есть
        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                if (attachment != null && attachment.Length > 0)
                {
                    // Копируем поток в MemoryStream, чтобы MailKit мог его прочитать
                    using var sourceStream = attachment.OpenReadStream();
                    var memoryStream = new MemoryStream();
                    await sourceStream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Position = 0;

                    var contentType = ContentType.Parse(attachment.ContentType ?? "application/octet-stream");
                    builder.Attachments.Add(attachment.FileName ?? "attachment", memoryStream, contentType);
                }
            }
        }

        // Важно: почтовые клиенты сами выберут HTML или Text.
        message.Body = builder.ToMessageBody();

        using var smtp = new MailKit.Net.Smtp.SmtpClient();
        await smtp.ConnectAsync(_options.SmtpHost, _options.SmtpPort, _options.SecureSocket, cancellationToken);
        if (smtp.Capabilities.HasFlag(MailKit.Net.Smtp.SmtpCapabilities.Authentication) &&
            !string.IsNullOrWhiteSpace(_options.SmtpLogin) &&
            !string.IsNullOrWhiteSpace(_options.SmtpPassword))
        {
            await smtp.AuthenticateAsync(_options.SmtpLogin, _options.SmtpPassword, cancellationToken);
        }

        try
        {
            await smtp.SendAsync(message, cancellationToken);
            _logger.LogInformation("Email sent to {Recipient} with {AttachmentCount} attachment(s)", toEmail, attachments?.Count() ?? 0);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error when sending email to {Recipient}", toEmail);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when sending email to {Recipient}", toEmail);
            throw;
        }
        finally
        {
            await smtp.DisconnectAsync(true, cancellationToken);
        }
    }

    public async Task<Dictionary<string, string>> SendAsyncWithContentIds(string toName, string toEmail, string subject, string textBody, string htmlBody, IEnumerable<IFormFile>? attachments, IEnumerable<KeyValuePair<string, string>>? contentIdMap, CancellationToken cancellationToken)
    {
         if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException("Recipient email is required.", nameof(toEmail));
        }

        var resultContentIdMap = new Dictionary<string, string>();

        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromUserName, _options.FromUserAddress));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            TextBody = textBody,
            HtmlBody = htmlBody,
        };

        // Добавляем вложения с Content-ID, если они есть
        if (attachments != null && contentIdMap != null)
        {
            var attachmentList = attachments.ToList();
            var contentIdList = contentIdMap.ToList();

            // Сопоставляем файлы с Content-ID по порядку
            for (int i = 0; i < attachmentList.Count && i < contentIdList.Count; i++)
            {
                var attachment = attachmentList[i];
                var contentIdPair = contentIdList[i];

                if (attachment != null && attachment.Length > 0)
                {
                    var contentId = contentIdPair.Key;
                    var fileName = contentIdPair.Value;
                    resultContentIdMap[contentId] = fileName;

                    // Копируем поток в MemoryStream, чтобы MailKit мог его прочитать
                    using var sourceStream = attachment.OpenReadStream();
                    var memoryStream = new MemoryStream();
                    await sourceStream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Position = 0;

                    var contentType = ContentType.Parse(attachment.ContentType ?? "application/octet-stream");
                    var attachmentEntity = builder.Attachments.Add(fileName, memoryStream, contentType);
                    attachmentEntity.ContentId = contentId;
                    attachmentEntity.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment);
                    attachmentEntity.ContentDisposition.FileName = fileName;
                }
            }
        }

        // Важно: почтовые клиенты сами выберут HTML или Text.
        message.Body = builder.ToMessageBody();

        using var smtp = new MailKit.Net.Smtp.SmtpClient();
        await smtp.ConnectAsync(_options.SmtpHost, _options.SmtpPort, _options.SecureSocket, cancellationToken);
        if (smtp.Capabilities.HasFlag(MailKit.Net.Smtp.SmtpCapabilities.Authentication) &&
            !string.IsNullOrWhiteSpace(_options.SmtpLogin) &&
            !string.IsNullOrWhiteSpace(_options.SmtpPassword))
        {
            await smtp.AuthenticateAsync(_options.SmtpLogin, _options.SmtpPassword, cancellationToken);
        }

        try
        {
            await smtp.SendAsync(message, cancellationToken);
            _logger.LogInformation("Email sent to {Recipient} with {AttachmentCount} attachment(s)", toEmail, attachments?.Count() ?? 0);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error when sending email to {Recipient}", toEmail);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when sending email to {Recipient}", toEmail);
            throw;
        }
        finally
        {
            await smtp.DisconnectAsync(true, cancellationToken);
        }

        return resultContentIdMap;
    }
}
