namespace AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;

internal interface IAnimalVaccinationRepository
{
    Task<AnimalVaccination?> GetByIdAsync(Guid id, Guid animalId, string shelterId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnimalVaccination>> ListByAnimalIdAsync(Guid animalId, string shelterId,
        CancellationToken cancellationToken = default);

    Task<AnimalVaccination?> FindReplacingAsync(Guid replacedId, string shelterId,
        CancellationToken cancellationToken = default);

    void Add(AnimalVaccination vaccination);

    void Remove(AnimalVaccination vaccination);
}
