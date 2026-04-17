namespace Cross.Messaging.Sms.Services;

/// <inheritdoc />
public class SmsSenderService : ISmsSenderService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SmsSenderService" /> class.
    /// </summary>
    public SmsSenderService()
    {

    }

    /// <inheritdoc />
    public Task<string> SendAsync(string destination, string body, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
