namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

public sealed class UpdateAnimalVaccinationRequest
{
    public const string Route = "/animals/{AnimalId}/vaccinations/{VaccinationId}";

    public Guid AnimalId { get; init; }
    public Guid VaccinationId { get; init; }
    public string VaccineName { get; init; } = string.Empty;
    public DateTimeOffset? AdministeredOn { get; init; }
    public DateTimeOffset? ValidUntil { get; init; }
    public string? Veterinarian { get; init; }
    public string? BatchNumber { get; init; }
    public string? Notes { get; init; }

    public static string BuildRoute(Guid animalId, Guid vaccinationId)
    {
        return Route.Replace("{AnimalId}", animalId.ToString())
            .Replace("{VaccinationId}", vaccinationId.ToString());
    }
}
