namespace Cross.Messaging.Tests.Sms.Extensions;

[Category("Unit")]
public sealed class SmsExtensionsTests
{
    private static IConfiguration BuildConfig(Action<Dictionary<string, string?>>? extra = null)
    {
        var d = new Dictionary<string, string?>
        {
            [$"{MessagingSmsOptions.SectionName}:DefaultProvider"] = "Telnyx",
            [$"{MessagingSmsOptions.SectionName}:CountryProviders:RO"] = "SMSClub",
            [$"{MessagingSmsOptions.SectionName}:Providers:Telnyx:ApiKey"] = "KEY",
            [$"{MessagingSmsOptions.SectionName}:Providers:Telnyx:From"] = "+15551234567",
            [$"{MessagingSmsOptions.SectionName}:Providers:Telnyx:MessagingProfileId"] = "",
            [$"{MessagingSmsOptions.SectionName}:Providers:SMSClub:ApiKey"] = "TOKEN",
            [$"{MessagingSmsOptions.SectionName}:Providers:SMSClub:From"] = "MySender",
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
        options.CountryProviders.Should().ContainKey("RO").WhoseValue.Should().Be("SMSClub");
        options.CountryProviders.Should().HaveCount(1);
        options.Providers.Should().ContainKey("Telnyx");
        options.Providers["Telnyx"].ApiKey.Should().Be("KEY");
        options.Providers["Telnyx"].From.Should().Be("+15551234567");
        options.Providers.Should().ContainKey("SMSClub");
        options.Providers["SMSClub"].ApiKey.Should().Be("TOKEN");
        options.Providers["SMSClub"].From.Should().Be("MySender");
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

    [Test]
    public void AddSmsSender_ShouldRegisterReferencedProvidersWithSettings()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<ILogger<SmsSenderService>>(NullLogger<SmsSenderService>.Instance);
        services.AddSmsSender(BuildConfig());

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var smsProviders = scope.ServiceProvider.GetServices<ISmsProvider>().ToList();

        smsProviders.Select(p => p.Name).Should().BeEquivalentTo("Telnyx", "SMSClub");
    }

    [Test]
    public void AddSmsSender_WhenOnlyTelnyxReferenced_ShouldNotRegisterSmsClub()
    {
        var config = BuildConfig(d =>
        {
            d.Remove($"{MessagingSmsOptions.SectionName}:CountryProviders:RO");
        });

        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<ILogger<SmsSenderService>>(NullLogger<SmsSenderService>.Instance);
        services.AddSmsSender(config);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var smsProviders = scope.ServiceProvider.GetServices<ISmsProvider>().ToList();

        smsProviders.Should().ContainSingle();
        smsProviders[0].Name.Should().Be("Telnyx");
        services.Any(s => s.ServiceType == typeof(SmsClubSmsProvider)).Should().BeFalse();
    }

    [Test]
    public void AddSmsSender_WhenOnlySmsClubReferenced_ShouldNotRegisterTelnyx()
    {
        var config = BuildConfig(d =>
        {
            d[$"{MessagingSmsOptions.SectionName}:DefaultProvider"] = "SMSClub";
            d.Remove($"{MessagingSmsOptions.SectionName}:CountryProviders:RO");
        });

        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<ILogger<SmsSenderService>>(NullLogger<SmsSenderService>.Instance);
        services.AddSmsSender(config);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var smsProviders = scope.ServiceProvider.GetServices<ISmsProvider>().ToList();

        smsProviders.Should().ContainSingle();
        smsProviders[0].Name.Should().Be("SMSClub");
        services.Any(s => s.ServiceType == typeof(TelnyxSmsProvider)).Should().BeFalse();
    }

    [Test]
    public void AddSmsSender_WhenReferencedButMissingProvidersSettings_ShouldNotRegister()
    {
        var config = BuildConfig(d =>
        {
            var keys = d.Keys.Where(k => k.Contains(":Providers:SMSClub:", StringComparison.Ordinal)).ToList();
            foreach (var key in keys)
            {
                d.Remove(key);
            }
        });

        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<ILogger<SmsSenderService>>(NullLogger<SmsSenderService>.Instance);
        services.AddSmsSender(config);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var smsProviders = scope.ServiceProvider.GetServices<ISmsProvider>().ToList();

        smsProviders.Should().ContainSingle();
        smsProviders[0].Name.Should().Be("Telnyx");
        services.Any(s => s.ServiceType == typeof(SmsClubSmsProvider)).Should().BeFalse();
    }

    [Test]
    public void AddSmsSender_WhenOnlyInProviders_ShouldNotRegisterUnreferenced()
    {
        var config = BuildConfig(d =>
        {
            d.Remove($"{MessagingSmsOptions.SectionName}:CountryProviders:RO");
        });

        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<ILogger<SmsSenderService>>(NullLogger<SmsSenderService>.Instance);
        services.AddSmsSender(config);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var smsProviders = scope.ServiceProvider.GetServices<ISmsProvider>().ToList();

        // SMSClub remains under Providers but is not referenced → not registered.
        smsProviders.Should().ContainSingle();
        smsProviders[0].Name.Should().Be("Telnyx");
        services.Any(s => s.ServiceType == typeof(SmsClubSmsProvider)).Should().BeFalse();
    }
}
