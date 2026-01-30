using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed class GetAnimalByIdQuery(Guid id) : IRequest<Result<GetAnimalByIdQueryResponse>>
{
    public Guid Id { get; } = id;
}

public sealed record GetAnimalByIdQueryResponse(Animal Animal);