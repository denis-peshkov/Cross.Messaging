namespace Cross.Messaging.Sms.Extensions;

/// <summary>
/// Dependency injection extensions for SMS messaging services.
/// </summary>
public static class SmsExtensions
{
    /// <summary>
    /// Registers SMS sender services and binds <see cref="MessagingSmsOptions" />.
    /// Registers a provider only when it is referenced by <c>DefaultProvider</c> or
    /// <c>CountryProviders</c> and also has settings under <c>MessagingSms:Providers</c>.
    /// Those references are evaluated once at startup; if a configuration reload later
    /// points to a provider that was not registered then, restart the application.
    /// </summary>
    /// <param name="services">Application service collection.</param>
    /// <param name="configuration">Application configuration source.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddSmsSender(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(MessagingSmsOptions.SectionName);
        services.Configure<MessagingSmsOptions>(section);

        if (HasConfiguredProvider(section, "Telnyx"))
        {
            services.AddHttpClient<TelnyxSmsProvider>(client =>
            {
                client.BaseAddress = new Uri("https://api.telnyx.com/");
            });
            services.TryAddEnumerable(ServiceDescriptor.Scoped<ISmsProvider, TelnyxSmsProvider>(sp =>
                sp.GetRequiredService<TelnyxSmsProvider>()));
        }

        if (HasConfiguredProvider(section, "SMSClub"))
        {
            services.AddHttpClient<SmsClubSmsProvider>(client =>
            {
                client.BaseAddress = new Uri("https://im.smsclub.mobi/");
            });
            services.TryAddEnumerable(ServiceDescriptor.Scoped<ISmsProvider, SmsClubSmsProvider>(sp =>
                sp.GetRequiredService<SmsClubSmsProvider>()));
        }

        services.TryAddScoped<ISmsSenderService, SmsSenderService>();

        return services;
    }

    private static bool HasConfiguredProvider(IConfigurationSection section, string providerName)
    {
        if (!IsReferencedProvider(section, providerName))
        {
            return false;
        }

        var providersSection = section.GetSection(nameof(MessagingSmsOptions.Providers));
        foreach (var child in providersSection.GetChildren())
        {
            if (string.Equals(child.Key, providerName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsReferencedProvider(IConfigurationSection section, string providerName)
    {
        if (string.Equals(section[nameof(MessagingSmsOptions.DefaultProvider)], providerName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var countryProvidersSection = section.GetSection(nameof(MessagingSmsOptions.CountryProviders));
        foreach (var child in countryProvidersSection.GetChildren())
        {
            if (string.Equals(child.Value, providerName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
