namespace AnimalRegistry.Modules.Animals.Api.AnimalHealth;

public sealed class UpdateAnimalHealthRequest
{
    public const string Route = "/animals/{AnimalId}/health/{HealthRecordId}";

    public Guid AnimalId { get; init; }
    public Guid HealthRecordId { get; init; }
    public DateTimeOffset OccurredOn { get; init; }
    public string Description { get; init; } = string.Empty;

    public static string BuildRoute(Guid animalId, Guid healthRecordId)
    {
        return Route.Replace("{AnimalId}", animalId.ToString())
            .Replace("{HealthRecordId}", healthRecordId.ToString());
    }
}