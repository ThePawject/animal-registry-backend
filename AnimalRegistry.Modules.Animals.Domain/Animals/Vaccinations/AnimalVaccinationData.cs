namespace AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;

public sealed record AnimalVaccinationData(
    string VaccineName,
    DateTimeOffset AdministeredOn,
    DateTimeOffset? ValidUntil,
    string? Veterinarian,
    string? BatchNumber,
    string? Notes);
