using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.AnimalHealth;

internal sealed class DeleteAnimalHealthCommand(Guid animalId, Guid healthRecordId) : IRequest<Result>
{
    public Guid AnimalId { get; } = animalId;
    public Guid HealthRecordId { get; } = healthRecordId;
}