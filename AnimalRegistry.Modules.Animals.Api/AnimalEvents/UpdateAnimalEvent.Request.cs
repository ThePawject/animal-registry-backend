using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;

namespace AnimalRegistry.Modules.Animals.Api.AnimalEvents;

public sealed class UpdateAnimalEventRequest
{
    public const string Route = "/animals/{AnimalId}/events/{EventId}";

    public Guid AnimalId { get; init; }
    public Guid EventId { get; init; }
    public AnimalEventType Type { get; init; }
    public DateTimeOffset OccurredOn { get; init; }
    public string Description { get; init; } = string.Empty;
    public string PerformedBy { get; init; } = string.Empty;

    public static string BuildRoute(Guid animalId, Guid eventId)
    {
        return Route.Replace("{AnimalId}", animalId.ToString()).Replace("{EventId}", eventId.ToString());
    }
}