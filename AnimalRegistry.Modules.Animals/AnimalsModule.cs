using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Infrastructure;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddMyMediator(typeof(CreateAnimalCommandHandler).Assembly);

        var connectionString = configuration.GetConnectionString("AnimalsDb")
                               ?? throw new InvalidOperationException("Connection string 'AnimalsDb' not found.");
        services.AddDbContext<AnimalsContext>(options => options.UseSqlServer(connectionString));

        // repositories
        services.AddScoped<IAnimalsRepository, AnimalsRepository>();
    }
}