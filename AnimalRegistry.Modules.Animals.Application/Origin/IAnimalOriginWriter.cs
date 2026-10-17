using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Shared;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

internal interface IAnimalOriginWriter
{
    Task<Result> ApplyAsync(Animal animal, AnimalOriginData? data, CancellationToken cancellationToken = default);
}
