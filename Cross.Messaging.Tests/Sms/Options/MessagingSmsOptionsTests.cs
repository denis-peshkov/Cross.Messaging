namespace Cross.Messaging.Tests.Sms.Options;

[Category("Unit")]
public sealed class MessagingSmsOptionsTests
{
    [Test]
    public void SectionName_ShouldMatchMessagingSmsKey()
    {
        MessagingSmsOptions.SectionName.Should().Be("MessagingSms");
    }

    [Test]
    public void Defaults_ShouldBeEmptyCollectionsAndNullProvider()
    {
        var o = new MessagingSmsOptions();

        o.DefaultProvider.Should().BeNull();
        o.CountryProviders.Should().NotBeNull().And.BeEmpty();
        o.Providers.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ProviderOptions_Defaults_ShouldBeNull()
    {
        var o = new MessagingSmsProviderOptions();

        o.ApiKey.Should().BeNull();
        o.From.Should().BeNull();
        o.MessagingProfileId.Should().BeNull();
    }
}
