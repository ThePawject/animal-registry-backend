using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.AnimalEvents;

internal sealed class DeleteAnimalEventCommand(Guid animalId, Guid eventId) : IRequest<Result>
{
    public Guid AnimalId { get; } = animalId;
    public Guid EventId { get; } = eventId;
}