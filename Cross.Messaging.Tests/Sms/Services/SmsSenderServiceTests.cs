namespace Cross.Messaging.Tests.Sms.Services;

public sealed class SmsSenderServiceTests
{
    private static readonly string[] Destinations =
    {
        "+10000000000",
        "+79990001122",
        "",
    };

    [Test]
    [TestCaseSource(nameof(Destinations))]
    public void SendAsync_ShouldThrowNotImplementedException(string destination)
    {
        var sut = new SmsSenderService();

        var act = () => sut.SendAsync(destination, "hello", CancellationToken.None);

        act.Should().ThrowAsync<NotImplementedException>();
    }
}
