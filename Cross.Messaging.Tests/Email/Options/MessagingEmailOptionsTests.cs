namespace Cross.Messaging.Tests.Email.Options;

public sealed class MessagingEmailOptionsTests
{
    [Test]
    public void SectionName_ShouldMatchMessagingEmailKey()
    {
        MessagingEmailOptions.SectionName.Should().Be("MessagingEmail");
    }

    [Test]
    public void Defaults_ShouldBeClrDefaults()
    {
        var o = new MessagingEmailOptions();

        o.SmtpHost.Should().BeNull();
        o.SmtpPort.Should().Be(0);
        o.UseSsl.Should().BeFalse();
        o.SmtpLogin.Should().BeNull();
        o.SmtpPassword.Should().BeNull();
        o.FromUserName.Should().BeNull();
        o.FromUserAddress.Should().BeNull();
        o.RecipientOverride.Should().BeNull();
        o.SecureSocket.Should().Be(default(SecureSocketOptions));
    }
}
