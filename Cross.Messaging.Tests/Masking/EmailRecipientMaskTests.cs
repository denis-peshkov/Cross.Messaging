namespace Cross.Messaging.Tests.Masking;

[Category("Unit")]
public sealed class EmailRecipientMaskTests
{
    [TestCase("dest@example.com", "d***@example.com")]
    [TestCase("john.doe@example.com", "j*******@example.com")]
    [TestCase("a@example.com", "*@example.com")]
    public void Mask_WhenValidEmail_KeepsFirstLocalCharAndDomain(string email, string expected)
    {
        var masked = EmailRecipientMask.Mask(email);

        masked.Should().Be(expected);
        masked.Should().NotBe(email);
    }

    [Test]
    public void Mask_WhenValidEmail_DoesNotContainFullLocalPart()
    {
        const string email = "dest@example.com";

        var masked = EmailRecipientMask.Mask(email);

        masked.Should().Be("d***@example.com");
        masked.Should().NotContain("est");
    }
}
