using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.AnimalEvents;

internal sealed class UpdateAnimalEventCommand(
    Guid animalId,
    Guid eventId,
    AnimalEventType type,
    DateTimeOffset occurredOn,
    string description) : IRequest<Result>
{
    public Guid AnimalId { get; } = animalId;
    public Guid EventId { get; } = eventId;
    public AnimalEventType Type { get; } = type;
    public DateTimeOffset OccurredOn { get; } = occurredOn;
    public string Description { get; } = description;
}