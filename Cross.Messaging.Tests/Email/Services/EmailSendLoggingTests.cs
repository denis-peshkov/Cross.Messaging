namespace Cross.Messaging.Tests.Email.Services;

public sealed class EmailSendLoggingTests
{
    [Test]
    public async Task RunSendAndLogAsync_WhenSendSucceeds_LogsInformation()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();

        await EmailSendLogging.RunSendAndLogAsync(() => Task.CompletedTask, "dest@example.com", logger.Object);

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
    public async Task RunSendAndLogAsync_WhenSendThrowsSmtpException_LogsErrorAndRethrows()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var smtpEx = new SmtpException(SmtpStatusCode.GeneralFailure);

        Func<Task> act = () => EmailSendLogging.RunSendAndLogAsync(
            () => Task.FromException(smtpEx),
            "dest@example.com",
            logger.Object);

        await act.Should().ThrowAsync<SmtpException>();

        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(e => e is SmtpException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task RunSendAndLogAsync_WhenSendThrowsNonSmtpException_LogsErrorAndRethrows()
    {
        var logger = new Mock<ILogger<EmailSenderService>>();
        var ex = new InvalidOperationException("fail");

        Func<Task> act = () => EmailSendLogging.RunSendAndLogAsync(
            () => Task.FromException(ex),
            "dest@example.com",
            logger.Object);

        await act.Should().ThrowAsync<InvalidOperationException>();

        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(e => e is InvalidOperationException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
