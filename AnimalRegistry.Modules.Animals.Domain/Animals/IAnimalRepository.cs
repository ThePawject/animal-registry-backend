using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals;

internal interface IAnimalRepository : IRepository<Animal>
{
    Task<IEnumerable<Animal>> ListAsync(CancellationToken cancellationToken = default);
}