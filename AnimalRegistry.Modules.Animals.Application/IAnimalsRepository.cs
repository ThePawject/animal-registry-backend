using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Application;

public interface IAnimalsRepository
{
    Task<List<Animal>> GetAllAsync();
    Task<Animal?> GetAsync(Guid id);
    Task<Animal> AddAsync(Animal animal);
}