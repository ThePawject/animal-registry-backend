using AnimalRegistry.Modules.Audit.Application.Services;
using AnimalRegistry.Modules.Audit.Domain.AuditEntries;
using AnimalRegistry.Modules.Audit.Infrastructure.Configuration;
using AnimalRegistry.Modules.Audit.Infrastructure.Interceptors;
using AnimalRegistry.Modules.Audit.Infrastructure.Persistence;
using AnimalRegistry.Modules.Audit.Infrastructure.Services;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.DDD;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalRegistry.Modules.Audit;

public class AuditModule : IModule
{
    public string Name => "Audit";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddFastEndpoints(options =>
        {
            options.Assemblies = [typeof(AuditModule).Assembly];
        });

        services.AddMediator(typeof(AuditModule).Assembly);

        services.Configure<AuditDatabaseSettings>(configuration.GetSection("Audit"));
        var connectionString = configuration.GetSection("Audit:ConnectionString").Value
                               ?? configuration.GetSection("Database:ConnectionString").Value
                               ?? throw new InvalidOperationException(
                                   "Audit database connection string not configured");

        services.AddDbContext<AuditDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IAuditEntryRepository, AuditEntryRepository>();

        services.AddHttpContextAccessor();
        services.AddScoped<IAuditMetadataProvider, AuditMetadataProvider>();
        services.AddScoped<IAuditService, AuditService>();

        RegisterDecorators(services);
    }

    public async Task MigrateAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        await context.Database.MigrateAsync();
    }

    private static void RegisterDecorators(IServiceCollection services)
    {
        services.Decorate<IDomainEventDispatcher>((inner, sp) =>
            new AuditDomainEventInterceptor(
                inner,
                sp.GetRequiredService<IAuditService>(),
                sp.GetRequiredService<ILogger<AuditDomainEventInterceptor>>()));

        services.Decorate<IMediator>((inner, sp) =>
            new AuditCommandInterceptor(
                inner,
                sp.GetRequiredService<IAuditService>(),
                sp.GetRequiredService<ILogger<AuditCommandInterceptor>>()));
    }
}