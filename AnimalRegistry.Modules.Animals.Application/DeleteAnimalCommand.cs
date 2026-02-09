using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed class DeleteAnimalCommand(Guid id) : IRequest<Result>
{
    public Guid Id { get; } = id;
}