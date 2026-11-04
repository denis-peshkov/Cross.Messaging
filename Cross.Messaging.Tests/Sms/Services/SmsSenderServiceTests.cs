namespace Cross.Messaging.Tests.Sms.Services;

[Category("Unit")]
public sealed class SmsSenderServiceTests
{
    [Test]
    public async Task SendAsync_WhenDestinationEmpty_ThrowsArgumentException()
    {
        var sut = CreateSut(CreateOptions(), CreateNamedProvider());

        var act = () => sut.SendAsync(" ", "hello", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("destination");
    }

    [Test]
    public async Task SendAsync_WhenBodyEmpty_ThrowsArgumentException()
    {
        var sut = CreateSut(CreateOptions(), CreateNamedProvider());

        var act = () => sut.SendAsync("+15551234567", " ", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("body");
    }

    [Test]
    public async Task SendAsync_WhenCountryMapped_UsesCountryProvider()
    {
        var provider = new Mock<ISmsProvider>();
        provider.SetupGet(p => p.Name).Returns("Telnyx");
        provider
            .Setup(p => p.SendAsync("+380501112233", "hi", It.IsAny<MessagingSmsProviderOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("id-ua");

        var options = CreateOptions();
        options.DefaultProvider = "Other";
        options.CountryProviders["UA"] = "Telnyx";

        var sut = CreateSut(options, provider.Object);
        var id = await sut.SendAsync("+380501112233", "hi", CancellationToken.None);

        id.Should().Be("id-ua");
        provider.Verify(
            p => p.SendAsync("+380501112233", "hi", It.IsAny<MessagingSmsProviderOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task SendAsync_WhenRoDestination_UsesSmsClubProvider()
    {
        var provider = new Mock<ISmsProvider>();
        provider.SetupGet(p => p.Name).Returns("SMSClub");
        provider
            .Setup(p => p.SendAsync("+40721234567", "hi", It.IsAny<MessagingSmsProviderOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("id-ro");

        var options = CreateOptions();
        options.DefaultProvider = "Telnyx";
        options.CountryProviders["RO"] = "SMSClub";
        options.Providers["SMSClub"] = new MessagingSmsProviderOptions
        {
            ApiKey = "TOKEN",
            From = "MySender",
        };

        var sut = CreateSut(options, provider.Object);
        var id = await sut.SendAsync("+40721234567", "hi", CancellationToken.None);

        id.Should().Be("id-ro");
        provider.Verify(
            p => p.SendAsync("+40721234567", "hi", It.IsAny<MessagingSmsProviderOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task SendAsync_WhenCountryNotMapped_UsesDefaultProvider()
    {
        var provider = new Mock<ISmsProvider>();
        provider.SetupGet(p => p.Name).Returns("Telnyx");
        provider
            .Setup(p => p.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessagingSmsProviderOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("id-default");

        var options = CreateOptions();
        options.DefaultProvider = "Telnyx";
        options.CountryProviders.Clear();

        var sut = CreateSut(options, provider.Object);
        var id = await sut.SendAsync("+380501112233", "hi", CancellationToken.None);

        id.Should().Be("id-default");
    }

    [Test]
    public async Task SendAsync_WhenProviderMissing_ThrowsInvalidOperationException()
    {
        var options = CreateOptions();
        options.DefaultProvider = "Missing";
        var sut = CreateSut(options);

        var act = () => sut.SendAsync("+15551234567", "hi", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not registered*");
    }

    private static MessagingSmsOptions CreateOptions()
    {
        return new MessagingSmsOptions
        {
            DefaultProvider = "Telnyx",
            CountryProviders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["RO"] = "SMSClub",
            },
            Providers = new Dictionary<string, MessagingSmsProviderOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["Telnyx"] = new MessagingSmsProviderOptions
                {
                    ApiKey = "KEY",
                    From = "+15550001111",
                },
                ["SMSClub"] = new MessagingSmsProviderOptions
                {
                    ApiKey = "TOKEN",
                    From = "MySender",
                },
            },
        };
    }

    private static ISmsProvider CreateNamedProvider(string name = "Telnyx")
    {
        var provider = new Mock<ISmsProvider>();
        provider.SetupGet(p => p.Name).Returns(name);
        return provider.Object;
    }

    private static SmsSenderService CreateSut(MessagingSmsOptions options, params ISmsProvider[] providers)
    {
        var snapshot = new Mock<IOptionsSnapshot<MessagingSmsOptions>>();
        snapshot.SetupGet(o => o.Value).Returns(options);
        return new SmsSenderService(
            NullLogger<SmsSenderService>.Instance,
            snapshot.Object,
            providers);
    }
}
