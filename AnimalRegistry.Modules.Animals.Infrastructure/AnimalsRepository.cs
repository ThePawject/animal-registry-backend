using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure;

public class AnimalsRepository(AnimalsContext context) : IAnimalsRepository
{
    public async Task<List<Animal>> GetAllAsync()
    {
        return await context.Animals.ToListAsync();
    }

    public async Task<Animal?> GetAsync(Guid id)
    {
        return await context.Animals.FindAsync(id);
    }

    public async Task<Animal> AddAsync(Animal animal)
    {
        var result = await context.AddAsync(animal);
        await context.SaveChangesAsync();
        return result.Entity;
    }
}