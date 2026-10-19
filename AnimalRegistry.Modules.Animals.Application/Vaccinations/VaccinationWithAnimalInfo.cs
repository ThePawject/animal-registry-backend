using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

public sealed record VaccinationWithAnimalInfo(
    AnimalVaccination Vaccination,
    string? AnimalName,
    string AnimalSignature,
    AnimalSpecies Species,
    bool IsInShelter,
    Guid? ReplacedByVaccinationId);
