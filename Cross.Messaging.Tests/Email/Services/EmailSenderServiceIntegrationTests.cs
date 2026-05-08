namespace Cross.Messaging.Tests.Email.Services;

[Category("Integration")]
public sealed class EmailSenderServiceIntegrationTests
{
    [Test]
    [Timeout(60_000)]
    public async Task SendAsync_WithSingleBody_DeliversToServer()
    {
        using var server = SimpleSmtpServer.Start();
        var port = server.Configuration.Port;

        var options = BuildOptions(port, useSsl: false);

        var logger = new Mock<ILogger<EmailSenderService>>();
        var sut = new EmailSenderService(logger.Object, options.Object);

        await sut.SendAsync("Recipient", "dest@example.com", "Subj", "<p>html</p>", CancellationToken.None);

        Assert.That(server.ReceivedEmailCount, Is.EqualTo(1));
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
    [Timeout(60_000)]
    public async Task SendAsync_WithSingleBody_AndEmptyToName_DeliversToServer()
    {
        using var server = SimpleSmtpServer.Start();
        var port = server.Configuration.Port;

        var options = BuildOptions(port, useSsl: false);

        var logger = new Mock<ILogger<EmailSenderService>>();
        var sut = new EmailSenderService(logger.Object, options.Object);

        await sut.SendAsync(string.Empty, "dest@example.com", "Subj", "body", CancellationToken.None);

        Assert.That(server.ReceivedEmailCount, Is.EqualTo(1));
    }

    [Test]
    [Timeout(60_000)]
    public async Task SendAsync_WithTextAndHtml_DeliversToServer()
    {
        using var server = SimpleSmtpServer.Start();
        var port = server.Configuration.Port;

        var options = BuildOptions(port, secureSocket: SecureSocketOptions.None);

        var logger = new Mock<ILogger<EmailSenderService>>();
        var sut = new EmailSenderService(logger.Object, options.Object);

        await sut.SendAsync("Recipient", "dest2@example.com", "Subj", "plain", "<b>html</b>", null, CancellationToken.None);

        Assert.That(server.ReceivedEmailCount, Is.EqualTo(1));
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
    [Timeout(60_000)]
    public async Task SendAsync_WithSingleBody_UsesRecipientOverride_WhenConfigured()
    {
        using var server = SimpleSmtpServer.Start();
        var port = server.Configuration.Port;
        var options = BuildOptions(port, useSsl: false, recipientOverride: "override@example.com");
        var logger = new Mock<ILogger<EmailSenderService>>();
        var sut = new EmailSenderService(logger.Object, options.Object);

        await sut.SendAsync("Recipient", "not-an-email", "Subj", "body", CancellationToken.None);

        Assert.That(server.ReceivedEmailCount, Is.EqualTo(1));
    }

    [Test]
    [Timeout(60_000)]
    public async Task SendAsync_WithTextAndHtml_AttachmentsAndOverride_DeliversToServer()
    {
        using var server = SimpleSmtpServer.Start();
        var port = server.Configuration.Port;
        var options = BuildOptions(port, secureSocket: SecureSocketOptions.None, recipientOverride: "override@example.com");
        var logger = new Mock<ILogger<EmailSenderService>>();
        var sut = new EmailSenderService(logger.Object, options.Object);

        var attachments = new List<IFormFile>
        {
            null!,
            BuildFormFile("empty.txt", "text/plain", string.Empty),
            BuildFormFile(null, null, "fallback"),
            BuildFormFile("payload.txt", "text/plain", "payload")
        };

        await sut.SendAsync("Recipient", "not-an-email", "Subj", "plain", "<b>html</b>", attachments, CancellationToken.None);

        Assert.That(server.ReceivedEmailCount, Is.EqualTo(1));
    }

    [Test]
    [Timeout(60_000)]
    public async Task SendAsyncWithContentIds_WithNullMaps_DeliversAndReturnsEmptyMap()
    {
        using var server = SimpleSmtpServer.Start();
        var port = server.Configuration.Port;
        var options = BuildOptions(port, secureSocket: SecureSocketOptions.None);
        var logger = new Mock<ILogger<EmailSenderService>>();
        var sut = new EmailSenderService(logger.Object, options.Object);

        var map = await sut.SendAsyncWithContentIds(
            "Recipient",
            "dest@example.com",
            "Subj",
            "plain",
            "<b>html</b>",
            null,
            null,
            CancellationToken.None);

        Assert.That(server.ReceivedEmailCount, Is.EqualTo(1));
        map.Should().BeEmpty();
    }

    [Test]
    [Timeout(60_000)]
    public async Task SendAsyncWithContentIds_WithAttachmentsAndOverride_DeliversAndReturnsMappedIds()
    {
        using var server = SimpleSmtpServer.Start();
        var port = server.Configuration.Port;
        var options = BuildOptions(port, secureSocket: SecureSocketOptions.None, recipientOverride: "override@example.com");
        var logger = new Mock<ILogger<EmailSenderService>>();
        var sut = new EmailSenderService(logger.Object, options.Object);

        var attachments = new List<IFormFile>
        {
            null!,
            BuildFormFile("empty.txt", "text/plain", string.Empty),
            BuildFormFile(null, null, "fallback"),
            BuildFormFile("img.png", "image/png", "png")
        };

        var contentIds = new List<KeyValuePair<string, string>>
        {
            new("cid-null", "null.txt"),
            new("cid-empty", "empty.txt"),
            new("cid-fallback", "fallback.bin"),
            new("cid-image", "img.png")
        };

        var map = await sut.SendAsyncWithContentIds(
            "Recipient",
            "not-an-email",
            "Subj",
            "plain",
            "<img src=\"cid:cid-image\" />",
            attachments,
            contentIds,
            CancellationToken.None);

        Assert.That(server.ReceivedEmailCount, Is.EqualTo(1));
        map.Should().ContainKey("cid-fallback");
        map.Should().ContainKey("cid-image");
        map["cid-fallback"].Should().Be("fallback.bin");
        map["cid-image"].Should().Be("img.png");
        map.Should().NotContainKey("cid-null");
        map.Should().NotContainKey("cid-empty");
    }

    [Test]
    [Timeout(60_000)]
    public async Task SendAsync_WithTextAndHtml_WhenServerSupportsAuth_AuthenticatesAndSends()
    {
        await using var server = await FakeAuthSmtpServer.StartAsync();
        var options = BuildOptions(server.Port, secureSocket: SecureSocketOptions.None);
        var logger = new Mock<ILogger<EmailSenderService>>();
        var sut = new EmailSenderService(logger.Object, options.Object);

        await sut.SendAsync(
            "Recipient",
            "dest@example.com",
            "Auth test",
            "plain",
            "<b>html</b>",
            null,
            CancellationToken.None);

        server.AuthenticatedSessions.Should().BeGreaterThan(0);
        server.AcceptedMessages.Should().BeGreaterThan(0);
    }

    [Test]
    [Timeout(60_000)]
    public async Task SendAsyncWithContentIds_WhenServerSupportsAuth_AuthenticatesAndSends()
    {
        await using var server = await FakeAuthSmtpServer.StartAsync();
        var options = BuildOptions(server.Port, secureSocket: SecureSocketOptions.None);
        var logger = new Mock<ILogger<EmailSenderService>>();
        var sut = new EmailSenderService(logger.Object, options.Object);

        var map = await sut.SendAsyncWithContentIds(
            "Recipient",
            "dest@example.com",
            "Auth content-id test",
            "plain",
            "<b>html</b>",
            null,
            null,
            CancellationToken.None);

        map.Should().BeEmpty();
        server.AuthenticatedSessions.Should().BeGreaterThan(0);
        server.AcceptedMessages.Should().BeGreaterThan(0);
    }

    [Test]
    [TestCase("", "p")]
    [TestCase("u", "")]
    [Timeout(60_000)]
    public async Task SendAsync_WithTextAndHtml_WhenCredentialsIncomplete_DoesNotAuthenticate(string login, string password)
    {
        await using var server = await FakeAuthSmtpServer.StartAsync();
        var options = BuildOptions(server.Port, secureSocket: SecureSocketOptions.None, login: login, password: password);
        var logger = new Mock<ILogger<EmailSenderService>>();
        var sut = new EmailSenderService(logger.Object, options.Object);

        await sut.SendAsync(
            "Recipient",
            "dest@example.com",
            "No auth test",
            "plain",
            "<b>html</b>",
            null,
            CancellationToken.None);

        server.AuthenticatedSessions.Should().Be(0);
        server.AcceptedMessages.Should().BeGreaterThan(0);
    }

    [Test]
    [TestCase("", "p")]
    [TestCase("u", "")]
    [Timeout(60_000)]
    public async Task SendAsyncWithContentIds_WhenCredentialsIncomplete_DoesNotAuthenticate(string login, string password)
    {
        await using var server = await FakeAuthSmtpServer.StartAsync();
        var options = BuildOptions(server.Port, secureSocket: SecureSocketOptions.None, login: login, password: password);
        var logger = new Mock<ILogger<EmailSenderService>>();
        var sut = new EmailSenderService(logger.Object, options.Object);

        var map = await sut.SendAsyncWithContentIds(
            "Recipient",
            "dest@example.com",
            "No auth content-id test",
            "plain",
            "<b>html</b>",
            null,
            null,
            CancellationToken.None);

        map.Should().BeEmpty();
        server.AuthenticatedSessions.Should().Be(0);
        server.AcceptedMessages.Should().BeGreaterThan(0);
    }

    private static Mock<IOptionsSnapshot<MessagingEmailOptions>> BuildOptions(
        int port,
        bool useSsl = false,
        SecureSocketOptions secureSocket = SecureSocketOptions.None,
        string? recipientOverride = null,
        string login = "u",
        string password = "p")
    {
        var options = new Mock<IOptionsSnapshot<MessagingEmailOptions>>();
        options.Setup(o => o.Value).Returns(new MessagingEmailOptions
        {
            SmtpHost = "127.0.0.1",
            SmtpPort = port,
            UseSsl = useSsl,
            SecureSocket = secureSocket,
            SmtpLogin = login,
            SmtpPassword = password,
            FromUserName = "Bot",
            FromUserAddress = "bot@example.com",
            RecipientOverride = recipientOverride ?? string.Empty,
        });
        return options;
    }

    private static IFormFile BuildFormFile(string? fileName, string? contentType, string payload)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(payload);
        var file = new Mock<IFormFile>();
        file.SetupGet(x => x.FileName).Returns(fileName!);
        file.SetupGet(x => x.ContentType).Returns(contentType!);
        file.SetupGet(x => x.Length).Returns(bytes.Length);
        file.Setup(x => x.OpenReadStream()).Returns(() => new MemoryStream(bytes));
        return file.Object;
    }

    private sealed class FakeAuthSmtpServer : IAsyncDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts;
        private Task _worker;

        private FakeAuthSmtpServer(TcpListener listener, CancellationTokenSource cts)
        {
            _listener = listener;
            _cts = cts;
            _worker = Task.CompletedTask;
        }

        public int Port { get; private init; }

        public int AuthenticatedSessions { get; private set; }

        public int AcceptedMessages { get; private set; }

        public static async Task<FakeAuthSmtpServer> StartAsync()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var cts = new CancellationTokenSource();
            var server = new FakeAuthSmtpServer(listener, cts)
            {
                Port = ((IPEndPoint)listener.LocalEndpoint).Port
            };

            server._worker = Task.Run(async () => await server.RunAsync(cts.Token), cts.Token);
            await Task.Yield();
            return server;
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient? client = null;
                try
                {
                    client = await _listener.AcceptTcpClientAsync(cancellationToken);
                    await HandleClientAsync(client, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                finally
                {
                    client?.Dispose();
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            await using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true)
            {
                NewLine = "\r\n",
                AutoFlush = true
            };

            await writer.WriteLineAsync("220 localhost ESMTP ready");

            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (line is null)
                {
                    break;
                }

                if (line.StartsWith("EHLO", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("HELO", StringComparison.OrdinalIgnoreCase))
                {
                    await writer.WriteLineAsync("250-localhost");
                    await writer.WriteLineAsync("250-AUTH LOGIN PLAIN");
                    await writer.WriteLineAsync("250 OK");
                    continue;
                }

                if (line.StartsWith("AUTH PLAIN", StringComparison.OrdinalIgnoreCase))
                {
                    AuthenticatedSessions++;
                    await writer.WriteLineAsync("235 2.7.0 Authentication successful");
                    continue;
                }

                if (line.StartsWith("AUTH LOGIN", StringComparison.OrdinalIgnoreCase))
                {
                    await writer.WriteLineAsync("334 VXNlcm5hbWU6");
                    _ = await reader.ReadLineAsync(); // username
                    await writer.WriteLineAsync("334 UGFzc3dvcmQ6");
                    _ = await reader.ReadLineAsync(); // password
                    AuthenticatedSessions++;
                    await writer.WriteLineAsync("235 2.7.0 Authentication successful");
                    continue;
                }

                if (line.StartsWith("MAIL FROM:", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("RCPT TO:", StringComparison.OrdinalIgnoreCase))
                {
                    await writer.WriteLineAsync("250 2.1.0 OK");
                    continue;
                }

                if (line.StartsWith("DATA", StringComparison.OrdinalIgnoreCase))
                {
                    await writer.WriteLineAsync("354 End data with <CR><LF>.<CR><LF>");
                    while (true)
                    {
                        var dataLine = await reader.ReadLineAsync();
                        if (dataLine is null || dataLine == ".")
                        {
                            break;
                        }
                    }

                    AcceptedMessages++;
                    await writer.WriteLineAsync("250 2.0.0 Accepted");
                    continue;
                }

                if (line.StartsWith("QUIT", StringComparison.OrdinalIgnoreCase))
                {
                    await writer.WriteLineAsync("221 2.0.0 Bye");
                    break;
                }

                await writer.WriteLineAsync("250 2.0.0 OK");
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _listener.Stop();
            try
            {
                await _worker;
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
            catch (ObjectDisposedException)
            {
                // expected on shutdown on some runtimes/platforms
            }
            catch (SocketException ex) when (
                ex.SocketErrorCode is SocketError.OperationAborted or SocketError.Interrupted or SocketError.InvalidArgument)
            {
                // expected on shutdown on some runtimes/platforms
            }
            finally
            {
                _cts.Dispose();
            }
        }
    }
}
