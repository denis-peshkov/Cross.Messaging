namespace Cross.Messaging.Tests.Contracts;

/// <summary>
/// Compile-time and reflection checks that public services match their abstractions.
/// </summary>
public sealed class ServiceContractsTests
{
    [Test]
    public void EmailSenderService_ShouldBeAssignableToIEmailSenderService()
    {
        typeof(EmailSenderService).IsAssignableTo(typeof(IEmailSenderService)).Should().BeTrue();
    }

    [Test]
    public void SmsSenderService_ShouldBeAssignableToISmsSenderService()
    {
        typeof(SmsSenderService).IsAssignableTo(typeof(ISmsSenderService)).Should().BeTrue();
    }
}
