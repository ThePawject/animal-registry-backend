using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.DomainEvents;

internal sealed class AnimalArchivedDomainEvent(Guid animalId): DomainEventBase
{
    public Guid AnimalId { get; } = animalId;
}