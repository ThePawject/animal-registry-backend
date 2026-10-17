namespace AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;

internal interface IAnimalVaccinationRepository
{
    Task<AnimalVaccination?> GetByIdAsync(Guid id, Guid animalId, string shelterId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnimalVaccination>> ListByAnimalIdAsync(Guid animalId, string shelterId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VaccinationWithAnimalInfo>> GetExpiringAsync(
        string shelterId,
        DateTimeOffset? validFrom,
        DateTimeOffset validBefore,
        bool inShelterOnly,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VaccinationWithAnimalInfo>> GetAdministeredBetweenAsync(
        string shelterId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    void Add(AnimalVaccination vaccination);

    void Remove(AnimalVaccination vaccination);
}
