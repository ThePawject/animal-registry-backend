using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Infrastructure;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AnimalRegistry.Modules.Animals;

public sealed class AnimalsModule : IModule
{
    public string Name => "Animals";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddFastEndpoints(options =>
        {
            options.Assemblies =
            [
                typeof(CreateAnimal).Assembly,
            ];
        });

        services.AddMediator(typeof(CreateAnimalCommand).Assembly);
        
        var connectionString = configuration.GetConnectionString("AnimalsDb");
        services.Configure<AnimalsDatabaseSettings>(options =>
        {
            options.ConnectionString = connectionString!;
        });
        services.AddDbContext<AnimalsDbContext>((serviceProvider, options) =>
        {
            var dbSettings = serviceProvider.GetRequiredService<IOptions<AnimalsDatabaseSettings>>().Value;
            options.UseSqlServer(dbSettings.ConnectionString);
        });
 
    }
}