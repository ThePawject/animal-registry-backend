using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

public sealed record VaccinationWithAnimalDto(
    AnimalVaccinationDto Vaccination,
    string? AnimalName,
    string AnimalSignature,
    AnimalSpecies Species,
    bool IsInShelter)
{
    public static VaccinationWithAnimalDto FromDomain(VaccinationWithAnimalInfo info, DateTimeOffset now,
        IBlobStorageService blobStorageService)
    {
        return new VaccinationWithAnimalDto(
            AnimalVaccinationDto.FromDomain(info.Vaccination, info.ReplacedByVaccinationId, now, blobStorageService),
            info.AnimalName,
            info.AnimalSignature,
            info.Species,
            info.IsInShelter);
    }
}
