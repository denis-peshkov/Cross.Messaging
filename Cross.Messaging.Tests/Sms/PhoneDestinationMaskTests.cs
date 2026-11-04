namespace Cross.Messaging.Tests.Sms;

[Category("Unit")]
public sealed class PhoneDestinationMaskTests
{
    [TestCase("+380501112233", "+380*****2233")]
    [TestCase("+15551234567", "+1******4567")]
    [TestCase("+40721234567", "+40*****4567")]
    public void Mask_WhenE164_KeepsCallingCodeAndLastDigits(string destination, string expected)
    {
        PhoneDestinationMask.Mask(destination).Should().Be(expected);
        PhoneDestinationMask.Mask(destination).Should().NotBe(destination);
        PhoneDestinationMask.Mask(destination).Should().NotContain(destination.Substring(destination.Length - 6, 2));
    }

    [Test]
    public void Mask_WhenE164_DoesNotContainFullNationalNumber()
    {
        const string destination = "+380501112233";

        var masked = PhoneDestinationMask.Mask(destination);

        masked.Should().Be("+380*****2233");
        masked.Should().NotContain("50111");
    }
}
