namespace Cross.Messaging.Tests.Masking;

[Category("Unit")]
public sealed class PhoneDestinationMaskTests
{
    [TestCase("+380501112233", "+380*****2233")]
    [TestCase("+15551234567", "+1******4567")]
    [TestCase("+40721234567", "+40*****4567")]
    [TestCase("380501112233", "+380*****2233")]
    [TestCase("40721234567", "+40*****4567")]
    public void Mask_WhenPhone_KeepsCallingCodeAndLastDigits(string destination, string expected)
    {
        var masked = PhoneDestinationMask.Mask(destination);

        masked.Should().Be(expected);
        masked.Should().NotBe(destination);
    }

    [Test]
    public void Mask_WhenE164_DoesNotContainFullNationalNumber()
    {
        const string destination = "+380501112233";

        var masked = PhoneDestinationMask.Mask(destination);

        masked.Should().Be("+380*****2233");
        masked.Should().NotContain("50111");
    }

    [Test]
    public void MaskIfPhone_WhenPhoneDigits_MasksNumber()
    {
        PhoneDestinationMask.MaskIfPhone("40721234567").Should().Be("+40*****4567");
    }

    [Test]
    public void MaskIfPhone_WhenDiagnosticKey_ReturnsUnchanged()
    {
        PhoneDestinationMask.MaskIfPhone("src_addr").Should().Be("src_addr");
    }

    [TestCase("40721234567", "+40721234567")]
    [TestCase("+40721234567", "+40721234567")]
    [TestCase(" 380501112233 ", "+380501112233")]
    public void NormalizeToE164_WhenPhone_PrefixesPlus(string value, string expected)
    {
        PhoneDestinationMask.NormalizeToE164(value).Should().Be(expected);
    }

    [Test]
    public void NormalizeToE164_WhenNotDigits_ReturnsTrimmed()
    {
        PhoneDestinationMask.NormalizeToE164(" src_addr ").Should().Be("src_addr");
    }

    [TestCase("+40721234567", true)]
    [TestCase("40721234567", true)]
    [TestCase("src_addr", false)]
    [TestCase("", false)]
    public void LooksLikePhoneNumber_DetectsPhoneLikeValues(string value, bool expected)
    {
        PhoneDestinationMask.LooksLikePhoneNumber(value).Should().Be(expected);
    }

    [Test]
    public void LooksLikePhoneNumber_WhenNull_ReturnsFalse()
    {
        PhoneDestinationMask.LooksLikePhoneNumber(null!).Should().BeFalse();
    }
}
