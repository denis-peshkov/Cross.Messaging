using Cross.Messaging.Email.Options;
using Cross.Messaging.Email.Services;

namespace Cross.Messaging.Tests.Email.Services;

public sealed class EmailSenderServiceTests
{
    [Test]
    public void SendAsync_WithSingleBody_ThrowsForEmptyEmail()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        var act = () => sut.SendAsync("name", "", "subject", "body", CancellationToken.None);

        act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public void SendAsync_WithTextAndHtml_ThrowsForEmptyEmail()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var options = BuildOptions();
        var sut = new EmailSenderService(logger.Object, options);

        var act = () => sut.SendAsync("name", "", "subject", "text", "<b>html</b>", CancellationToken.None);

        act.Should().ThrowAsync<ArgumentException>();
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
