namespace Cross.Messaging.Tests.Sms.Extensions;

[Category("Unit")]
public sealed class SmsExtensionsTests
{
    private static IConfiguration BuildConfig(Action<Dictionary<string, string?>>? extra = null)
    {
        var d = new Dictionary<string, string?>
        {
            [$"{MessagingSmsOptions.SectionName}:DefaultProvider"] = "Telnyx",
            [$"{MessagingSmsOptions.SectionName}:CountryProviders:US"] = "Telnyx",
            [$"{MessagingSmsOptions.SectionName}:CountryProviders:UA"] = "Telnyx",
            [$"{MessagingSmsOptions.SectionName}:Providers:Telnyx:ApiKey"] = "KEY",
            [$"{MessagingSmsOptions.SectionName}:Providers:Telnyx:From"] = "+15551234567",
            [$"{MessagingSmsOptions.SectionName}:Providers:Telnyx:MessagingProfileId"] = "",
        };
        extra?.Invoke(d);
        return new ConfigurationBuilder().AddInMemoryCollection(d).Build();
    }

    [Test]
    public void AddSmsSender_ShouldRegisterServiceAndBindOptions()
    {
        var services = new ServiceCollection();
        var config = BuildConfig();

        var returned = services.AddSmsSender(config);
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<MessagingSmsOptions>>().Value;
        var senderDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ISmsSenderService));

        returned.Should().BeSameAs(services);
        options.DefaultProvider.Should().Be("Telnyx");
        options.CountryProviders.Should().ContainKey("US").WhoseValue.Should().Be("Telnyx");
        options.Providers.Should().ContainKey("Telnyx");
        options.Providers["Telnyx"].ApiKey.Should().Be("KEY");
        options.Providers["Telnyx"].From.Should().Be("+15551234567");
        senderDescriptor.Should().NotBeNull();
        senderDescriptor!.ImplementationType.Should().Be(typeof(SmsSenderService));
    }

    [Test]
    public void AddSmsSender_ShouldRegisterSmsSenderAsScoped()
    {
        var services = new ServiceCollection();
        services.AddSmsSender(BuildConfig());

        var descriptor = services.Single(s => s.ServiceType == typeof(ISmsSenderService));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Test]
    public void AddSmsSender_CalledTwice_ShouldNotDuplicateRegistration()
    {
        var services = new ServiceCollection();
        var config = BuildConfig();

        services.AddSmsSender(config);
        services.AddSmsSender(config);

        services.Count(s => s.ServiceType == typeof(ISmsSenderService)).Should().Be(1);
    }

    [Test]
    public void AddSmsSender_ShouldResolveSenderWithinScope()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<ILogger<SmsSenderService>>(NullLogger<SmsSenderService>.Instance);
        services.AddSmsSender(BuildConfig());

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISmsSenderService>();

        sender.Should().BeOfType<SmsSenderService>();
    }
}
