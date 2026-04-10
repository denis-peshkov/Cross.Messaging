using Cross.Messaging.Email.Extensions;
using Cross.Messaging.Email.Options;
using Cross.Messaging.Email.Services;

namespace Cross.Messaging.Tests.Email.Extensions;

public sealed class EmailExtensionsTests
{
    [Test]
    public void AddEmailSender_ShouldRegisterServiceAndBindOptions()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{MessagingEmailOptions.SectionName}:SmtpHost"] = "smtp.example.com",
                [$"{MessagingEmailOptions.SectionName}:SmtpPort"] = "587",
                [$"{MessagingEmailOptions.SectionName}:UseSsl"] = "true",
                [$"{MessagingEmailOptions.SectionName}:SmtpLogin"] = "login",
                [$"{MessagingEmailOptions.SectionName}:SmtpPassword"] = "password",
                [$"{MessagingEmailOptions.SectionName}:FromUserName"] = "Bot",
                [$"{MessagingEmailOptions.SectionName}:FromUserAddress"] = "bot@example.com",
            })
            .Build();

        var returned = services.AddEmailSender(config);
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<MessagingEmailOptions>>().Value;
        var senderDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IEmailSenderService));

        returned.Should().BeSameAs(services);
        options.SmtpHost.Should().Be("smtp.example.com");
        options.SmtpPort.Should().Be(587);
        options.UseSsl.Should().BeTrue();
        senderDescriptor.Should().NotBeNull();
        senderDescriptor!.ImplementationType.Should().Be(typeof(EmailSenderService));
    }
}
