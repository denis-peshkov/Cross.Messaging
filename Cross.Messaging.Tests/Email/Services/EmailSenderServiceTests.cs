using System.IO;

namespace Cross.Messaging.Tests.Email.Services;

[Category("Unit")]
public sealed class EmailSenderServiceTests
{
    private static readonly object?[] InvalidRecipientEmailCases =
    {
        "",
        "   ",
        "\t",
        "\n",
        null,
    };

    [Test]
    [TestCaseSource(nameof(InvalidRecipientEmailCases))]
    public async Task SendAsync_WithSingleBody_ThrowsArgumentExceptionForInvalidEmail(string? toEmail)
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        Func<Task> act = () => sut.SendAsync("name", toEmail!, "subject", "body", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(toEmail))
            .Where(e => e.Message.Contains("Recipient email", StringComparison.Ordinal));
    }

    [Test]
    [TestCaseSource(nameof(InvalidRecipientEmailCases))]
    public async Task SendAsync_WithTextAndHtml_ThrowsArgumentExceptionForInvalidEmail(string? toEmail)
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        Func<Task> act = () => sut.SendAsync("name", toEmail!, "subject", "text", "<b>html</b>", null, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(toEmail))
            .Where(e => e.Message.Contains("Recipient email", StringComparison.Ordinal));
    }

    [Test]
    [TestCaseSource(nameof(InvalidRecipientEmailCases))]
    public async Task SendAsync_WithTextAndHtmlWithoutAttachments_ThrowsArgumentExceptionForInvalidEmail(string? toEmail)
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        Func<Task> act = () => sut.SendAsync("name", toEmail!, "subject", "text", "<b>html</b>", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(toEmail))
            .Where(e => e.Message.Contains("Recipient email", StringComparison.Ordinal));
    }

    [Test]
    [TestCaseSource(nameof(InvalidRecipientEmailCases))]
    public async Task SendAsync_WithTextAndHtmlAndPriorityWithoutAttachments_ThrowsArgumentExceptionForInvalidEmail(string? toEmail)
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        var priorityType = Type.GetType("MimeKit.XMessagePriority, MimeKit");
        priorityType.Should().NotBeNull();

        var method = typeof(EmailSenderService).GetMethod(
            nameof(EmailSenderService.SendAsync),
            new[]
            {
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                priorityType!,
                typeof(CancellationToken),
            });
        method.Should().NotBeNull();
        var priorityValue = Enum.ToObject(priorityType!, 1);

        Func<Task> act = async () => await (Task)method!.Invoke(sut, new object?[] { "name", toEmail!, "subject", "text", "<b>html</b>", priorityValue, CancellationToken.None })!;

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(toEmail))
            .Where(e => e.Message.Contains("Recipient email", StringComparison.Ordinal));
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task SendAsync_WithSingleBody_ThrowsArgumentExceptionForInvalidSubject(string? subject)
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        Func<Task> act = () => sut.SendAsync("name", "dest@example.com", subject!, "body", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(subject));
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task SendAsync_WithSingleBody_ThrowsArgumentExceptionForInvalidBody(string? body)
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        Func<Task> act = () => sut.SendAsync("name", "dest@example.com", "subject", body!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(body));
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task SendAsync_WithTextAndHtml_ThrowsArgumentExceptionForInvalidSubject(string? subject)
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        Func<Task> act = () => sut.SendAsync("name", "dest@example.com", subject!, "text", "<b>html</b>", null, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(subject));
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task SendAsync_WithTextAndHtml_ThrowsArgumentExceptionForInvalidTextBody(string? textBody)
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        Func<Task> act = () => sut.SendAsync("name", "dest@example.com", "subject", textBody!, "<b>html</b>", null, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(textBody));
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task SendAsync_WithTextAndHtml_ThrowsArgumentExceptionForInvalidHtmlBody(string? htmlBody)
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        Func<Task> act = () => sut.SendAsync("name", "dest@example.com", "subject", "text", htmlBody!, null, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(htmlBody));
    }

    /// <summary>
    /// System.Net.Mail path: failure happens inside <c>SendMailAsync</c> and is wrapped into <see cref="InvalidOperationException" />.
    /// </summary>
    [Test]
    [Timeout(15_000)]
    public async Task SendAsync_WithSingleBody_WhenSmtpUnreachable_ThrowsInvalidOperationException()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var value = new MessagingEmailOptions
        {
            SmtpHost = "127.0.0.1",
            SmtpPort = 1,
            UseSsl = false,
            SmtpLogin = "x",
            SmtpPassword = "y",
            FromUserName = "Bot",
            FromUserAddress = "bot@example.com",
        };
        var options = new Mock<IOptionsSnapshot<MessagingEmailOptions>>();
        options.Setup(x => x.Value).Returns(value);
        var sut = new EmailSenderService(logger.Object, options.Object);

        Func<Task> act = () => sut.SendAsync("Name", "dest@example.com", "subj", "body", CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<InvalidOperationException>();
        exceptionAssertion.Which.Message.Should().Contain("SMTP error");
        exceptionAssertion.Which.InnerException.Should().NotBeNull();
    }

    [Test]
    [Timeout(15_000)]
    public async Task SendAsync_WithSingleBody_WhenSmtpUnreachableAndBccConfigured_ThrowsInvalidOperationException()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var value = new MessagingEmailOptions
        {
            SmtpHost = "127.0.0.1",
            SmtpPort = 1,
            UseSsl = false,
            SmtpLogin = "x",
            SmtpPassword = "y",
            FromUserName = "Bot",
            FromUserAddress = "bot@example.com",
            BccRecipients = new List<MessagingEmailBccRecipientOptions>
            {
                new MessagingEmailBccRecipientOptions
                {
                    Name = "Audit",
                    Email = "audit@example.com",
                },
                new MessagingEmailBccRecipientOptions
                {
                    Name = "Skipped",
                    Email = " ",
                }
            },
        };
        var options = new Mock<IOptionsSnapshot<MessagingEmailOptions>>();
        options.Setup(x => x.Value).Returns(value);
        var sut = new EmailSenderService(logger.Object, options.Object);

        Func<Task> act = () => sut.SendAsync("Name", "dest@example.com", "subj", "body", CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<InvalidOperationException>();
        exceptionAssertion.Which.Message.Should().Contain("SMTP error");
        exceptionAssertion.Which.InnerException.Should().NotBeNull();
    }

    /// <summary>
    /// MailKit path: <c>ConnectAsync</c> fails before the inner try — exception is not wrapped/logged by this service.
    /// </summary>
    [Test]
    [Timeout(30_000)]
    public async Task SendAsync_WithTextAndHtml_WhenConnectFails_ThrowsBeforeSend()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var value = new MessagingEmailOptions
        {
            SmtpHost = "127.0.0.1",
            SmtpPort = 1,
            SecureSocket = SecureSocketOptions.None,
            SmtpLogin = "x",
            SmtpPassword = "y",
            FromUserName = "Bot",
            FromUserAddress = "bot@example.com",
        };
        var options = new Mock<IOptionsSnapshot<MessagingEmailOptions>>();
        options.Setup(x => x.Value).Returns(value);
        var sut = new EmailSenderService(logger.Object, options.Object);

        Func<Task> act = () => sut.SendAsync("Name", "dest@example.com", "subj", "text", "<p>x</p>", null, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    [Test]
    [Timeout(30_000)]
    public async Task SendAsync_WithTextAndHtmlAndPriorityWithoutAttachments_WhenConnectFailsAndBccConfigured_ThrowsBeforeSend()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var value = new MessagingEmailOptions
        {
            SmtpHost = "127.0.0.1",
            SmtpPort = 1,
            SecureSocket = SecureSocketOptions.None,
            SmtpLogin = "x",
            SmtpPassword = "y",
            FromUserName = "Bot",
            FromUserAddress = "bot@example.com",
            BccRecipients = new List<MessagingEmailBccRecipientOptions>
            {
                new MessagingEmailBccRecipientOptions
                {
                    Name = "Audit",
                    Email = "audit@example.com",
                },
                new MessagingEmailBccRecipientOptions
                {
                    Name = "Skipped",
                    Email = " ",
                }
            },
        };
        var options = new Mock<IOptionsSnapshot<MessagingEmailOptions>>();
        options.Setup(x => x.Value).Returns(value);
        var sut = new EmailSenderService(logger.Object, options.Object);

        var priorityType = Type.GetType("MimeKit.XMessagePriority, MimeKit");
        priorityType.Should().NotBeNull();

        var method = typeof(EmailSenderService).GetMethod(
            nameof(EmailSenderService.SendAsync),
            new[]
            {
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                priorityType!,
                typeof(CancellationToken),
            });
        method.Should().NotBeNull();
        var priorityValue = Enum.ToObject(priorityType!, 1);

        Func<Task> act = async () => await (Task)method!.Invoke(sut, new object?[] { "Name", "dest@example.com", "subj", "text", "<p>x</p>", priorityValue, CancellationToken.None })!;

        await act.Should().ThrowAsync<Exception>();
    }

    [Test]
    public async Task SendAsyncWithContentIds_WhenEmailIsInvalid_ThrowsArgumentException()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        Func<Task> act = () => sut.SendAsyncWithContentIds(
            "name",
            "",
            "subject",
            "text",
            "<b>html</b>",
            null,
            null,
            CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("toEmail");
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task SendAsyncWithContentIds_WhenSubjectIsInvalid_ThrowsArgumentException(string? subject)
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        Func<Task> act = () => sut.SendAsyncWithContentIds(
            "name",
            "dest@example.com",
            subject!,
            "text",
            "<b>html</b>",
            null,
            null,
            CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(subject));
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task SendAsyncWithContentIds_WhenTextBodyIsInvalid_ThrowsArgumentException(string? textBody)
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        Func<Task> act = () => sut.SendAsyncWithContentIds(
            "name",
            "dest@example.com",
            "subject",
            textBody!,
            "<b>html</b>",
            null,
            null,
            CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(textBody));
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task SendAsyncWithContentIds_WhenHtmlBodyIsInvalid_ThrowsArgumentException(string? htmlBody)
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        Func<Task> act = () => sut.SendAsyncWithContentIds(
            "name",
            "dest@example.com",
            "subject",
            "text",
            htmlBody!,
            null,
            null,
            CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(htmlBody));
    }

    [Test]
    public async Task SendAsyncWithContentIds_WhenConnectFails_ReturnsExceptionAfterPreparingAttachments()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var value = new MessagingEmailOptions
        {
            SmtpHost = "127.0.0.1",
            SmtpPort = 1,
            SecureSocket = SecureSocketOptions.None,
            SmtpLogin = "x",
            SmtpPassword = "y",
            FromUserName = "Bot",
            FromUserAddress = "bot@example.com",
        };

        var options = new Mock<IOptionsSnapshot<MessagingEmailOptions>>();
        options.Setup(x => x.Value).Returns(value);
        var sut = new EmailSenderService(logger.Object, options.Object);

        var attachment = BuildFormFile("file.txt", "text/plain", "payload");
        var attachments = new[] { attachment };
        var contentMap = new[] { new KeyValuePair<string, string>("cid-1", "file.txt") };

        Func<Task> act = async () => await sut.SendAsyncWithContentIds(
            "Name",
            "dest@example.com",
            "subj",
            "text",
            "<p>x</p>",
            attachments,
            contentMap,
            CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    private static IOptionsSnapshot<MessagingEmailOptions> BuildOptions()
    {
        var value = new MessagingEmailOptions
        {
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            UseSsl = true,
            SmtpLogin = "login",
            SmtpPassword = "password",
            FromUserName = "Bot",
            FromUserAddress = "bot@example.com",
        };

        var options = new Mock<IOptionsSnapshot<MessagingEmailOptions>>();
        options.Setup(x => x.Value).Returns(value);
        return options.Object;
    }

    private static IFormFile BuildFormFile(string fileName, string contentType, string payload)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(payload);
        var file = new Mock<IFormFile>();
        file.SetupGet(x => x.FileName).Returns(fileName);
        file.SetupGet(x => x.ContentType).Returns(contentType);
        file.SetupGet(x => x.Length).Returns(bytes.Length);
        file.Setup(x => x.OpenReadStream()).Returns(() => new MemoryStream(bytes));
        return file.Object;
    }

}
