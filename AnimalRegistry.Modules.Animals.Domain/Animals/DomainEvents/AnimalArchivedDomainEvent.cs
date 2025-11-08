using AnimalRegistry.Shared.DDD;

internal sealed class AnimalArchivedDomainEvent(Guid animalId): IDomainEvent
{
    public Guid AnimalId { get; } = animalId;
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}