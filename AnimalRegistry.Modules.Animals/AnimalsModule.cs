using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;
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

        services.AddMediator(typeof(CreateAnimalCommand).Assembly);
    }
}