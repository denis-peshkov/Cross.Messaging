namespace Cross.Messaging.Email.Services;

/// <inheritdoc />
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
            // ReplyToList = { new MailAddress() },
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
            _logger,
            toEmail
        );
    }

    /// <inheritdoc />
    public async Task SendAsync(string toName, string toEmail, string subject, string textBody, string htmlBody, IReadOnlyCollection<IFormFile>? attachments, CancellationToken cancellationToken)
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

        // Note: add attachments, if exists
        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                if (attachment != null && attachment.Length > 0)
                {
                    // Copy Stream to MemoryStream so MailKit can read it
                    await using var sourceStream = attachment.OpenReadStream();
                    var memoryStream = new MemoryStream();
                    await sourceStream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Position = 0;

                    var contentType = ContentType.Parse(attachment.ContentType ?? "application/octet-stream");
                    await builder.Attachments.AddAsync(attachment.FileName ?? "attachment", memoryStream, contentType, cancellationToken);
                }
            }
        }

        // Note: mail clients choose whether to show HTML or plain text
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
            await EmailSendLogging.RunSendAndLogAsync(
                () => smtp.SendAsync(message, cancellationToken),
                _logger,
                toEmail,
                attachments
            );
        }
        finally
        {
            await smtp.DisconnectAsync(true, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, string>> SendAsyncWithContentIds(string toName, string toEmail, string subject, string textBody, string htmlBody, IReadOnlyCollection<IFormFile>? attachments, IReadOnlyCollection<KeyValuePair<string, string>>? contentIdMap, CancellationToken cancellationToken)
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

        // Add attachments with Content-ID, when provided
        if (attachments != null && contentIdMap != null)
        {
            var attachmentList = attachments.ToList();
            var contentIdList = contentIdMap.ToList();

            // Match files to Content-ID entries by index order
            for (var i = 0; i < attachmentList.Count && i < contentIdList.Count; i++)
            {
                var attachment = attachmentList[i];

                if (attachment is not { Length: > 0 })
                {
                    continue;
                }

                var (contentId, fileName) = contentIdList[i];
                resultContentIdMap[contentId] = fileName;

                // Copy Stream to MemoryStream so MailKit can read it
                await using var sourceStream = attachment.OpenReadStream();
                var memoryStream = new MemoryStream();
                await sourceStream.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;

                var contentType = ContentType.Parse(attachment.ContentType ?? "application/octet-stream");
                var attachmentEntity = await builder.Attachments.AddAsync(fileName, memoryStream, contentType, cancellationToken);
                attachmentEntity.ContentId = contentId;
                attachmentEntity.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment)
                {
                    FileName = fileName
                };
            }
        }

        // Note: mail clients choose whether to show HTML or plain text
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
            await EmailSendLogging.RunSendAndLogAsync(
                () => smtp.SendAsync(message, cancellationToken),
                _logger,
                toEmail,
                attachments
            );
        }
        finally
        {
            await smtp.DisconnectAsync(true, cancellationToken);
        }

        return resultContentIdMap;
    }
}
