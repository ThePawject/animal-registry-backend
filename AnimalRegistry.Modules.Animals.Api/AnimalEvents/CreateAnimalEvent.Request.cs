using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;

namespace AnimalRegistry.Modules.Animals.Api.AnimalEvents;

public sealed class CreateAnimalEventRequest
{
    public const string Route = "/animals/{AnimalId}/events";

    public Guid AnimalId { get; init; }
    public AnimalEventType Type { get; init; }
    public DateTimeOffset OccurredOn { get; init; }
    public string Description { get; init; } = string.Empty;
    
    public static string BuildRoute(Guid animalId)
    {
        return Route.Replace("{AnimalId}", animalId.ToString());
    }
}