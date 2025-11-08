using AnimalRegistry.Shared.DDD;

internal sealed class AnimalArchivedDomainEvent(Guid animalId): DomainEventBase
{
    public Guid AnimalId { get; } = animalId;
}