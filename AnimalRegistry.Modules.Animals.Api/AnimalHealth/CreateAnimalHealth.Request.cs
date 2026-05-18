using Microsoft.AspNetCore.Http;

namespace AnimalRegistry.Modules.Animals.Api.AnimalHealth;

public sealed class CreateAnimalHealthRequest
{
    public const string Route = "/animals/{AnimalId}/health";

    public Guid AnimalId { get; init; }
    public DateTimeOffset OccurredOn { get; init; }
    public string Description { get; init; } = string.Empty;
    public IFormFile? DocumentFile { get; init; }

    public static string BuildRoute(Guid animalId)
    {
        return Route.Replace("{AnimalId}", animalId.ToString());
    }
}