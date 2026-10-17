using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

public interface IAnimalOriginReader
{
    Task<AnimalOriginLookup?> LoadAsync(IReadOnlyCollection<Animal> animals,
        CancellationToken cancellationToken = default);
}
