namespace Cross.Messaging.Tests.Sms;

[Category("Unit")]
public sealed class PhoneCountryResolverTests
{
    [TestCase("+15551234567", "US")]
    [TestCase("+380501112233", "UA")]
    [TestCase("+447911123456", "GB")]
    [TestCase("+4915123456789", "DE")]
    [TestCase("+8613812345678", "CN")]
    [TestCase("+40721234567", "RO")]
    public void TryResolve_WhenValidE164_ReturnsExpectedCountry(string destination, string expected)
    {
        var ok = PhoneCountryResolver.TryResolve(destination, out var country);

        ok.Should().BeTrue();
        country.Should().Be(expected);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("15551234567")]
    [TestCase("+")]
    [TestCase("+12ab")]
    [TestCase(null)]
    public void TryResolve_WhenInvalid_ReturnsFalse(string? destination)
    {
        var ok = PhoneCountryResolver.TryResolve(destination!, out var country);

        ok.Should().BeFalse();
        country.Should().BeNull();
    }

    [Test]
    public void TryResolve_UsesLongestPrefix_ForThreeDigitCodes()
    {
        var ok = PhoneCountryResolver.TryResolve("+380501112233", out var country);

        ok.Should().BeTrue();
        country.Should().Be("UA");
    }
}
