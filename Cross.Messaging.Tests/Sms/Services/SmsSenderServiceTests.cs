using Cross.Messaging.Sms.Services;

namespace Cross.Messaging.Tests.Sms.Services;

public sealed class SmsSenderServiceTests
{
    [Test]
    public void SendAsync_ShouldThrowNotImplementedException()
    {
        var sut = new SmsSenderService();

        var act = () => sut.SendAsync("+10000000000", "hello", CancellationToken.None);

        act.Should().ThrowAsync<NotImplementedException>();
    }
}
