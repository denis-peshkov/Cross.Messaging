namespace Cross.Messaging.Tests.Email.Services;

public sealed class EmailSenderServiceIntegrationTests
{
    [Test]
    [Timeout(60_000)]
    public async Task SendAsync_WithSingleBody_DeliversToServer()
    {
        using var server = SimpleSmtpServer.Start();
        var port = server.Configuration.Port;

        var options = new Mock<IOptionsSnapshot<MessagingEmailOptions>>();
        options.Setup(o => o.Value).Returns(new MessagingEmailOptions
        {
            SmtpHost = "127.0.0.1",
            SmtpPort = port,
            UseSsl = false,
            SmtpLogin = "u",
            SmtpPassword = "p",
            FromUserName = "Bot",
            FromUserAddress = "bot@example.com",
        });

        var logger = new Mock<ILogger<EmailSenderService>>();
        var sut = new EmailSenderService(logger.Object, options.Object);

        await sut.SendAsync("Recipient", "dest@example.com", "Subj", "<p>html</p>", CancellationToken.None);

        Assert.That(server.ReceivedEmailCount, Is.EqualTo(1));
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    [Timeout(60_000)]
    public async Task SendAsync_WithSingleBody_AndEmptyToName_DeliversToServer()
    {
        using var server = SimpleSmtpServer.Start();
        var port = server.Configuration.Port;

        var options = new Mock<IOptionsSnapshot<MessagingEmailOptions>>();
        options.Setup(o => o.Value).Returns(new MessagingEmailOptions
        {
            SmtpHost = "127.0.0.1",
            SmtpPort = port,
            UseSsl = false,
            SmtpLogin = "u",
            SmtpPassword = "p",
            FromUserName = "Bot",
            FromUserAddress = "bot@example.com",
        });

        var logger = new Mock<ILogger<EmailSenderService>>();
        var sut = new EmailSenderService(logger.Object, options.Object);

        await sut.SendAsync(string.Empty, "dest@example.com", "Subj", "body", CancellationToken.None);

        Assert.That(server.ReceivedEmailCount, Is.EqualTo(1));
    }

    [Test]
    [Timeout(60_000)]
    public async Task SendAsync_WithTextAndHtml_DeliversToServer()
    {
        using var server = SimpleSmtpServer.Start();
        var port = server.Configuration.Port;

        var options = new Mock<IOptionsSnapshot<MessagingEmailOptions>>();
        options.Setup(o => o.Value).Returns(new MessagingEmailOptions
        {
            SmtpHost = "127.0.0.1",
            SmtpPort = port,
            SecureSocket = SecureSocketOptions.None,
            SmtpLogin = "u",
            SmtpPassword = "p",
            FromUserName = "Bot",
            FromUserAddress = "bot@example.com",
        });

        var logger = new Mock<ILogger<EmailSenderService>>();
        var sut = new EmailSenderService(logger.Object, options.Object);

        await sut.SendAsync("Recipient", "dest2@example.com", "Subj", "plain", "<b>html</b>", CancellationToken.None);

        Assert.That(server.ReceivedEmailCount, Is.EqualTo(1));
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
