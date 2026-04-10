namespace Cross.Messaging.Tests.Email.Extensions;

public sealed class EmailExtensionsTests
{
    private static IConfiguration BuildConfig(Action<Dictionary<string, string?>>? extra = null)
    {
        var d = new Dictionary<string, string?>
        {
            [$"{MessagingEmailOptions.SectionName}:SmtpHost"] = "smtp.example.com",
            [$"{MessagingEmailOptions.SectionName}:SmtpPort"] = "587",
            [$"{MessagingEmailOptions.SectionName}:UseSsl"] = "true",
            [$"{MessagingEmailOptions.SectionName}:SmtpLogin"] = "login",
            [$"{MessagingEmailOptions.SectionName}:SmtpPassword"] = "password",
            [$"{MessagingEmailOptions.SectionName}:FromUserName"] = "Bot",
            [$"{MessagingEmailOptions.SectionName}:FromUserAddress"] = "bot@example.com",
        };
        extra?.Invoke(d);
        return new ConfigurationBuilder().AddInMemoryCollection(d).Build();
    }

    [Test]
    public void AddEmailSender_ShouldRegisterServiceAndBindOptions()
    {
        var services = new ServiceCollection();
        var config = BuildConfig();

        var returned = services.AddEmailSender(config);
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<MessagingEmailOptions>>().Value;
        var senderDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IEmailSenderService));

        returned.Should().BeSameAs(services);
        options.SmtpHost.Should().Be("smtp.example.com");
        options.SmtpPort.Should().Be(587);
        options.UseSsl.Should().BeTrue();
        senderDescriptor.Should().NotBeNull();
        senderDescriptor!.ImplementationType.Should().Be(typeof(EmailSenderService));
    }

    [Test]
    public void AddEmailSender_ShouldBindRecipientOverride_WhenPresent()
    {
        var services = new ServiceCollection();
        var config = BuildConfig(d => d[$"{MessagingEmailOptions.SectionName}:RecipientOverride"] = "override@example.com");

        services.AddEmailSender(config);
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<MessagingEmailOptions>>().Value;

        options.RecipientOverride.Should().Be("override@example.com");
    }

    [Test]
    public void AddEmailSender_ShouldRegisterEmailSenderAsScoped()
    {
        var services = new ServiceCollection();
        services.AddEmailSender(BuildConfig());

        var descriptor = services.Single(s => s.ServiceType == typeof(IEmailSenderService));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Test]
    public void AddEmailSender_CalledTwice_ShouldNotDuplicateRegistration()
    {
        var services = new ServiceCollection();
        var config = BuildConfig();

        services.AddEmailSender(config);
        services.AddEmailSender(config);

        services.Count(s => s.ServiceType == typeof(IEmailSenderService)).Should().Be(1);
    }

    [Test]
    public void AddEmailSender_ShouldResolveSameInstanceWithinScope()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<ILogger<EmailSenderService>>(NullLogger<EmailSenderService>.Instance);
        services.AddEmailSender(BuildConfig());
        using var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var a = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();
        var b = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();

        a.Should().BeSameAs(b);
    }

    [Test]
    public void AddEmailSender_ShouldResolveDifferentInstancesAcrossScopes()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<ILogger<EmailSenderService>>(NullLogger<EmailSenderService>.Instance);
        services.AddEmailSender(BuildConfig());
        using var provider = services.BuildServiceProvider();

        IEmailSenderService a;
        using (var scope = provider.CreateScope())
        {
            a = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();
        }

        using (var scope = provider.CreateScope())
        {
            var b = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();
            b.Should().NotBeSameAs(a);
        }
    }
}
