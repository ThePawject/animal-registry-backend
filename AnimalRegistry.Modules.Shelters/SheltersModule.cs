using AnimalRegistry.Modules.Shelters.Api;
using AnimalRegistry.Modules.Shelters.Application;
using AnimalRegistry.Modules.Shelters.Domain;
using AnimalRegistry.Modules.Shelters.Infrastructure;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace AnimalRegistry.Modules.Shelters;

public sealed class SheltersModule : IModule
{
    public string Name => "Shelters";

    public IReadOnlyCollection<Assembly> EndpointAssemblies => [typeof(GetShelterFeatures).Assembly];

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediator(typeof(GetShelterFeaturesQueryHandler).Assembly);

        services.Configure<SheltersDatabaseSettings>(options =>
        {
            options.ConnectionString = configuration["Database:ConnectionString"]!;
        });
        services.AddDbContext<SheltersDbContext>((serviceProvider, options) =>
        {
            var dbSettings = serviceProvider.GetRequiredService<IOptions<SheltersDatabaseSettings>>().Value;
            options.UseSqlServer(dbSettings.ConnectionString,
                sqlServer => sqlServer.MigrationsHistoryTable(SheltersDbContext.MigrationsHistoryTable));
        });

        services.AddScoped<IShelterFeatureActivationRepository, ShelterFeatureActivationRepository>();
        services.AddShelterFeatureToggles<ShelterFeatureToggles>();
    }

    public async Task MigrateAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SheltersDbContext>();

        var pending = await db.Database.GetPendingMigrationsAsync();

        if (!pending.Any())
        {
            return;
        }

        await db.Database.MigrateAsync();
    }
}
