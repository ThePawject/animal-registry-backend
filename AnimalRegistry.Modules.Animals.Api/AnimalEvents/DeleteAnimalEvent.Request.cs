namespace AnimalRegistry.Modules.Animals.Api.AnimalEvents;

public sealed class DeleteAnimalEventRequest
{
    public const string Route = "/animals/{AnimalId}/events/{EventId}";

    public Guid AnimalId { get; init; }
    public Guid EventId { get; init; }

    public static string BuildRoute(Guid animalId, Guid eventId)
    {
        return Route.Replace("{AnimalId}", animalId.ToString()).Replace("{EventId}", eventId.ToString());
    }
}