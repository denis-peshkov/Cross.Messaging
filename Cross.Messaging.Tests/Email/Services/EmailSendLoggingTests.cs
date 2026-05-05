namespace Cross.Messaging.Tests.Email.Services;

public sealed class EmailSendLoggingTests
{
    [Test]
    public async Task RunSendAndLogAsync_WhenSendSucceeds_LogsInformation()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();

        await EmailSendLogging.RunSendAndLogAsync(() => Task.CompletedTask, logger.Object, "dest@example.com");

        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task RunSendAndLogAsync_WhenSendThrowsSmtpException_WrapsIntoInvalidOperationException()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var smtpEx = new SmtpException(SmtpStatusCode.GeneralFailure);

        Func<Task> act = () => EmailSendLogging.RunSendAndLogAsync(
            () => Task.FromException(smtpEx),
            logger.Object,
            "dest@example.com");

        var exceptionAssertion = await act.Should().ThrowAsync<InvalidOperationException>();
        exceptionAssertion.Which.InnerException.Should().BeOfType<SmtpException>();
        exceptionAssertion.Which.Message.Should().Contain("SMTP error");
    }

    [Test]
    public async Task RunSendAndLogAsync_WhenSendThrowsNonSmtpException_WrapsIntoInvalidOperationException()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var ex = new InvalidOperationException("fail");

        Func<Task> act = () => EmailSendLogging.RunSendAndLogAsync(
            () => Task.FromException(ex),
            logger.Object,
            "dest@example.com");

        var exceptionAssertion = await act.Should().ThrowAsync<InvalidOperationException>();
        exceptionAssertion.Which.InnerException.Should().BeSameAs(ex);
        exceptionAssertion.Which.Message.Should().Contain("Unexpected error");
    }

    [Test]
    public async Task RunSendAndLogAsync_WhenAttachmentsProvided_LogsInformation()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var attachments = new[] { new Mock<IFormFile>().Object, new Mock<IFormFile>().Object };

        await EmailSendLogging.RunSendAndLogAsync(() => Task.CompletedTask, logger.Object, "dest@example.com", attachments);

        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
