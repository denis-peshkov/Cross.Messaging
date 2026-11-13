namespace Cross.Messaging.Tests.Sms.Providers;

[Category("Unit")]
public sealed class SmsClubSmsProviderTests
{
    [Test]
    public void Name_ShouldBeSmsClub()
    {
        var sut = new SmsClubSmsProvider(new HttpClient());

        sut.Name.Should().Be("SMSClub");
    }

    [Test]
    public async Task SendAsync_WhenSuccess_ReturnsMessageId()
    {
        HttpRequestMessage? captured = null;
        string? requestBody = null;
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>(async (request, _) =>
            {
                captured = request;
                requestBody = request.Content is null
                    ? null
                    : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """{"success_request":{"info":{"106":"40721234567"}}}""",
                        Encoding.UTF8,
                        "application/json"),
                };
            });

        var http = new HttpClient(handler.Object) { BaseAddress = new Uri("https://im.smsclub.mobi/") };
        var sut = new SmsClubSmsProvider(http);
        var options = new MessagingSmsProviderOptions
        {
            ApiKey = "TOKEN",
            From = "MySender",
        };

        var id = await sut.SendAsync("+40721234567", "hello", options, CancellationToken.None);

        id.Should().Be("106");
        captured.Should().NotBeNull();
        captured!.Method.Should().Be(HttpMethod.Post);
        captured.RequestUri!.ToString().Should().Contain("sms/send");
        captured.Headers.Authorization!.Scheme.Should().Be("Bearer");
        captured.Headers.Authorization.Parameter.Should().Be("TOKEN");
        requestBody.Should().Contain("40721234567");
        requestBody.Should().NotContain("+40721234567");
        requestBody.Should().Contain("MySender");
        requestBody.Should().Contain("hello");
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
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("""{"error":"unauthorized"}"""),
            });

        var sut = new SmsClubSmsProvider(new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://im.smsclub.mobi/"),
        });
        var options = new MessagingSmsProviderOptions
        {
            ApiKey = "TOKEN",
            From = "MySender",
        };

        var act = () => sut.SendAsync("+40721234567", "hello", options, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("401");
        exception.Which.Message.Should().Contain("unauthorized");
    }

    [Test]
    public async Task SendAsync_WhenAddInfoOnly_ThrowsWithAddInfoDetails()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"success_request":{"add_info":{"src_addr":"Sender name is incorrect"}}}"""),
            });

        var sut = new SmsClubSmsProvider(new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://im.smsclub.mobi/"),
        });
        var options = new MessagingSmsProviderOptions
        {
            ApiKey = "TOKEN",
            From = "Bad",
        };

        var act = () => sut.SendAsync("+40721234567", "hello", options, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("src_addr");
        exception.Which.Message.Should().Contain("Sender name is incorrect");
        exception.Which.Message.Should().NotContain("message id is missing");
    }

    [Test]
    public async Task SendAsync_WhenPhoneRejectedInAddInfo_ThrowsWithPhoneError()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"success_request":{"add_info":{"40721234567":"Number is blacklisted"}}}"""),
            });

        var sut = new SmsClubSmsProvider(new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://im.smsclub.mobi/"),
        });
        var options = new MessagingSmsProviderOptions
        {
            ApiKey = "TOKEN",
            From = "MySender",
        };

        var act = () => sut.SendAsync("+40721234567", "hello", options, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("40721234567");
        exception.Which.Message.Should().Contain("Number is blacklisted");
    }

    [Test]
    public async Task SendAsync_WhenInfoAndAddInfoMissing_ThrowsMessageIdMissing()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"success_request":{}}"""),
            });

        var sut = new SmsClubSmsProvider(new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://im.smsclub.mobi/"),
        });
        var options = new MessagingSmsProviderOptions
        {
            ApiKey = "TOKEN",
            From = "MySender",
        };

        var act = () => sut.SendAsync("+40721234567", "hello", options, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*message id is missing*");
    }

    [Test]
    public async Task SendAsync_WhenApiKeyMissing_ThrowsInvalidOperationException()
    {
        var sut = new SmsClubSmsProvider(new HttpClient());
        var options = new MessagingSmsProviderOptions { From = "MySender" };

        var act = () => sut.SendAsync("+40721234567", "hello", options, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ApiKey*");
    }
}
