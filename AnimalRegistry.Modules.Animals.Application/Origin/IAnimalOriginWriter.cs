using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

internal interface IAnimalOriginWriter
{
    Task<Result> ApplyAsync(Animal animal, AnimalOriginInput? input, CancellationToken cancellationToken = default);
}
