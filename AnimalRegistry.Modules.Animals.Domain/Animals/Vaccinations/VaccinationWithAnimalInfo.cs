namespace AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;

public sealed record VaccinationWithAnimalInfo(
    AnimalVaccination Vaccination,
    string? AnimalName,
    string AnimalSignature,
    AnimalSpecies Species,
    bool IsInShelter);
