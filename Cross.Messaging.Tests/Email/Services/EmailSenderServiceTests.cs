namespace Cross.Messaging.Tests.Email.Services;

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

        Func<Task> act = () => sut.SendAsync("name", toEmail!, "subject", "text", "<b>html</b>", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(toEmail))
            .Where(e => e.Message.Contains("Recipient email", StringComparison.Ordinal));
    }

    /// <summary>
    /// System.Net.Mail path: failure happens inside <c>SendMailAsync</c>, inside the service try/catch — error is logged.
    /// </summary>
    [Test]
    [Timeout(15_000)]
    public async Task SendAsync_WithSingleBody_WhenSmtpUnreachable_LogsErrorAndThrows()
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

        await act.Should().ThrowAsync<Exception>();

        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
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

        Func<Task> act = () => sut.SendAsync("Name", "dest@example.com", "subj", "text", "<p>x</p>", CancellationToken.None);

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
}
