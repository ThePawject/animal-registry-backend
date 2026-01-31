using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class GetAnimalQuery(Guid id) : IRequest<Result<AnimalDto>>
{
    public Guid Id { get; } = id;
}
