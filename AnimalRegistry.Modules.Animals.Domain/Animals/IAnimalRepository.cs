using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Pagination;

namespace AnimalRegistry.Modules.Animals.Domain.Animals;

internal interface IAnimalRepository
{
    Task<Animal?> GetByIdAsync(Guid id, string shelterId, CancellationToken cancellationToken = default);

    Task<PagedResult<Animal>> ListAsync(string shelterId, int page, int pageSize,
        CancellationToken cancellationToken = default);

    Task<Result<Animal>> AddAsync(Animal entity, CancellationToken cancellationToken = default);

    Task<Result<Animal>> UpdateAsync(Animal entity, CancellationToken cancellationToken = default);
    
    void Remove(Animal entity);
}