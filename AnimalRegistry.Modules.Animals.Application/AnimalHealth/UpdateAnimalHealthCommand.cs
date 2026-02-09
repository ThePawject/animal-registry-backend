using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.AnimalHealth;

internal sealed class UpdateAnimalHealthCommand(
    Guid animalId,
    Guid healthRecordId,
    DateTimeOffset occurredOn,
    string description) : IRequest<Result>
{
    public Guid AnimalId { get; } = animalId;
    public Guid HealthRecordId { get; } = healthRecordId;
    public DateTimeOffset OccurredOn { get; } = occurredOn;
    public string Description { get; } = description;
}