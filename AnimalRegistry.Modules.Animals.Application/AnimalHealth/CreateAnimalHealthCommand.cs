using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.AnimalHealth;

internal sealed class CreateAnimalHealthCommand(
    Guid animalId,
    DateTimeOffset occurredOn,
    string description) : IRequest<Result>
{
    public Guid AnimalId { get; } = animalId;
    public DateTimeOffset OccurredOn { get; } = occurredOn;
    public string Description { get; } = description;
}