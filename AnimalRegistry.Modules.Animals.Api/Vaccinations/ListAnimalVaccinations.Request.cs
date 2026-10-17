namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

public sealed class ListAnimalVaccinationsRequest
{
    public const string Route = "/animals/{AnimalId}/vaccinations";

    public Guid AnimalId { get; init; }

    public static string BuildRoute(Guid animalId)
    {
        return Route.Replace("{AnimalId}", animalId.ToString());
    }
}
