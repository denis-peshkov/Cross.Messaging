namespace Cross.Messaging.Sms.Extensions;

/// <summary>
/// Dependency injection extensions for SMS messaging services.
/// </summary>
public static class SmsExtensions
{
    /// <summary>
    /// Registers SMS sender services and binds <see cref="MessagingSmsOptions" />.
    /// </summary>
    /// <param name="services">Application service collection.</param>
    /// <param name="configuration">Application configuration source.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddSmsSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MessagingSmsOptions>(configuration.GetSection(MessagingSmsOptions.SectionName));

        services.AddHttpClient<TelnyxSmsProvider>(client =>
        {
            client.BaseAddress = new Uri("https://api.telnyx.com/");
        });

        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISmsProvider, TelnyxSmsProvider>(sp =>
            sp.GetRequiredService<TelnyxSmsProvider>()));

        services.TryAddScoped<ISmsSenderService, SmsSenderService>();

        return services;
    }
}
