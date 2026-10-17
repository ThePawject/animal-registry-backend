namespace AnimalRegistry.Modules.Animals.Domain.Animals.Origin;

internal interface IAnimalOriginRepository
{
    Task<AnimalOrigin?> GetByAnimalIdAsync(Guid animalId, string shelterId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnimalOrigin>> GetByAnimalIdsAsync(IEnumerable<Guid> animalIds, string shelterId,
        CancellationToken cancellationToken = default);

    void Add(AnimalOrigin origin);
}
