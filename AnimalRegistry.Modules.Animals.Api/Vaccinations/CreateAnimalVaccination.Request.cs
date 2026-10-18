namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

public sealed class CreateAnimalVaccinationRequest
{
    public const string Route = "/animals/{AnimalId}/vaccinations";

    public Guid AnimalId { get; init; }
    public string VaccineName { get; init; } = string.Empty;
    public DateTimeOffset? AdministeredOn { get; init; }
    public DateTimeOffset? ValidUntil { get; init; }
    public string? Veterinarian { get; init; }
    public string? BatchNumber { get; init; }
    public string? Notes { get; init; }
    public Guid? ReplacesVaccinationId { get; init; }

    public static string BuildRoute(Guid animalId)
    {
        return Route.Replace("{AnimalId}", animalId.ToString());
    }
}
