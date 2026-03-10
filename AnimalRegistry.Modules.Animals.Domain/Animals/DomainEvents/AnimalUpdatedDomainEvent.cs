using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.DomainEvents;

internal sealed class AnimalUpdatedDomainEvent(Guid animalId, string name, string signature) : DomainEventBase
{
    public Guid AnimalId { get; } = animalId;
    public string Name { get; } = name;
    public string Signature { get; } = signature;
}