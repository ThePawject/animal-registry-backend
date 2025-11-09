using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.DomainEvents;

internal sealed class AnimalCreatedDomainEvent(Guid animalId, string signature, string name): DomainEventBase
{
    public Guid AnimalId { get; } = animalId;
    public string Signature { get; } = signature;
    public string Name { get; } = name;
}