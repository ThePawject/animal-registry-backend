using AnimalRegistry.Shared.DDD;

internal sealed class AnimalCreatedDomainEvent(Guid animalId, string signature, string name): IDomainEvent
{
    public Guid AnimalId { get; } = animalId;
    public string Signature { get; } = signature;
    public string Name { get; } = name;
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}