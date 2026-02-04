using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals;

internal interface IAnimalRepository : IRepository<Animal>
{
    Task<Animal?> GetByIdAsync(Guid id, string shelterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Animal>> ListAsync(string shelterId, CancellationToken cancellationToken = default);
}