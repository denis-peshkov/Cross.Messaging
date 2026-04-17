namespace Cross.Messaging.Email.Extensions;

/// <summary>
/// Dependency injection extensions for email messaging services.
/// </summary>
public static class EmailExtensions
{
    /// <summary>
    /// Registers email sender services and binds <see cref="MessagingEmailOptions" />.
    /// </summary>
    /// <param name="services">Application service collection.</param>
    /// <param name="configuration">Application configuration source.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MessagingEmailOptions>(configuration.GetSection(MessagingEmailOptions.SectionName));
        services.TryAddScoped<IEmailSenderService, EmailSenderService>();

        return services;
    }
}
