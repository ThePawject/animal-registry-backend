using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Modules.Animals;

public sealed class AnimalsModule: IModule
{
    public string Name { get; } = "Animals";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddFastEndpoints(options =>
        {
            options.Assemblies =
            [
                typeof(CreateAnimal).Assembly,
            ];
        });

        services.AddMyMediator(typeof(CreateAnimal).Assembly);
    }
}