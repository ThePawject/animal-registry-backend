using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals;

public interface IAnimalRepository : IRepository<Animal>
{
    // Możesz dodać dodatkowe metody specyficzne dla Animal
}
