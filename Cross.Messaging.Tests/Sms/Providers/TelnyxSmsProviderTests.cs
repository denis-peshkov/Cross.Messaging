namespace Cross.Messaging.Tests.Sms.Providers;

[Category("Unit")]
public sealed class TelnyxSmsProviderTests
{
    [Test]
    public void Name_ShouldBeTelnyx()
    {
        var sut = new TelnyxSmsProvider(new HttpClient());

        sut.Name.Should().Be("Telnyx");
    }

    [Test]
    public async Task SendAsync_WhenSuccess_ReturnsMessageId()
    {
        HttpRequestMessage? captured = null;
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                captured = request;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """{"data":{"id":"msg-123"}}""",
                        Encoding.UTF8,
                        "application/json"),
                };
            });

        var http = new HttpClient(handler.Object) { BaseAddress = new Uri("https://api.telnyx.com/") };
        var sut = new TelnyxSmsProvider(http);
        var options = new MessagingSmsProviderOptions
        {
            ApiKey = "KEY",
            From = "+15550001111",
        };

        var id = await sut.SendAsync("+15551234567", "hello", options, CancellationToken.None);

        id.Should().Be("msg-123");
        captured.Should().NotBeNull();
        captured!.Method.Should().Be(HttpMethod.Post);
        captured.RequestUri!.ToString().Should().Contain("v2/messages");
        captured.Headers.Authorization!.Scheme.Should().Be("Bearer");
        captured.Headers.Authorization.Parameter.Should().Be("KEY");
    }

    [Test]
    public async Task SendAsync_WhenApiReturnsError_ThrowsInvalidOperationException()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("""{"errors":[{"detail":"bad"}]}"""),
            });

        var sut = new TelnyxSmsProvider(new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.telnyx.com/"),
        });
        var options = new MessagingSmsProviderOptions
        {
            ApiKey = "KEY",
            From = "+15550001111",
        };

        var act = () => sut.SendAsync("+15551234567", "hello", options, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*400*");
    }

    [Test]
    public async Task SendAsync_WhenApiKeyMissing_ThrowsInvalidOperationException()
    {
        var sut = new TelnyxSmsProvider(new HttpClient());
        var options = new MessagingSmsProviderOptions { From = "+15550001111" };

        var act = () => sut.SendAsync("+15551234567", "hello", options, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ApiKey*");
    }
}
