using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.DomainEvents;

public sealed class AnimalEventAddedDomainEvent(Guid animalId, AnimalEvent animalEvent) : DomainEventBase
{
    public Guid AnimalId { get; } = animalId;
    public AnimalEvent AnimalEvent { get; } = animalEvent;
}