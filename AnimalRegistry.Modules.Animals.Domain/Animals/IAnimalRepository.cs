using AnimalRegistry.Shared;

namespace AnimalRegistry.Modules.Animals.Domain.Animals;

internal interface IAnimalRepository
{
    Task<Animal?> GetByIdAsync(Guid id, string shelterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Animal>> ListAsync(string shelterId, CancellationToken cancellationToken = default);
    Task<Result<Animal>> AddAsync(Animal entity, CancellationToken cancellationToken = default);
    void Remove(Animal entity);
}