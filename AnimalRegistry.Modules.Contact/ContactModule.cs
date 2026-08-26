using AnimalRegistry.Modules.Contact.Api;
using AnimalRegistry.Modules.Contact.Application;
using AnimalRegistry.Modules.Contact.Domain;
using AnimalRegistry.Modules.Contact.Infrastructure;
using AnimalRegistry.Modules.Contact.Infrastructure.Email;
using AnimalRegistry.Modules.Contact.RateLimiting;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace AnimalRegistry.Modules.Contact;

public sealed class ContactModule : IModule
{
    private const string MigrationsHistoryTable = "__EFMigrationsHistoryContact";

    public string Name => "Contact";

    public IReadOnlyCollection<Assembly> EndpointAssemblies => [typeof(SubmitContactRequest).Assembly];

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddMediator(typeof(SubmitContactRequestCommandHandler).Assembly);
        services.TryAddSingleton(TimeProvider.System);

        services.Configure<ContactDatabaseSettings>(options =>
        {
            options.ConnectionString = configuration["Database:ConnectionString"]!;
        });
        services.AddDbContext<ContactDbContext>((serviceProvider, options) =>
        {
            var dbSettings = serviceProvider.GetRequiredService<IOptions<ContactDatabaseSettings>>().Value;

            options.UseSqlServer(
                dbSettings.ConnectionString,
                sqlServer => sqlServer.MigrationsHistoryTable(MigrationsHistoryTable));
        });
        services.AddScoped<IContactRequestRepository, ContactRequestRepository>();

        RegisterEmail(services, configuration);

        var rateLimitSettings = configuration.GetSection(ContactRateLimitSettings.SectionName)
                                    .Get<ContactRateLimitSettings>()
                                ?? new ContactRateLimitSettings();
        services.AddContactRateLimiter(rateLimitSettings);
    }

    public async Task MigrateAsync(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();

        var pending = await db.Database.GetPendingMigrationsAsync();

        if (!pending.Any())
        {
            return;
        }

        await db.Database.MigrateAsync();
    }

    private static void RegisterEmail(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection(EmailSettings.SectionName))
            .ValidateOnStart();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<EmailSettings>, EmailSettingsValidator>());

        services.AddSingleton<IContactEmailFactory, ContactEmailFactory>();
        services.AddSingleton<IEmailSender>(sp => sp.GetRequiredService<IOptions<EmailSettings>>().Value.Enabled
            ? ActivatorUtilities.CreateInstance<SmtpEmailSender>(sp)
            : ActivatorUtilities.CreateInstance<NoOpEmailSender>(sp));
    }
}