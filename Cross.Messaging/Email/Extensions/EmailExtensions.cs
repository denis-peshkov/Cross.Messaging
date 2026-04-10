namespace Cross.Messaging.Email.Extensions;

public static class EmailExtensions
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MessagingEmailOptions>(configuration.GetSection(MessagingEmailOptions.SectionName));
        services.TryAddScoped<IEmailSenderService, EmailSenderService>();

        return services;
    }
}
