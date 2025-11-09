using AnimalRegistry.Modules.Animals.Domain.Animals;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals;

public class AnimalRepository(AnimalsDbContext context) : IAnimalRepository
{
    public async Task<Animal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Animals.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task AddAsync(Animal entity, CancellationToken cancellationToken = default)
    {
        await context.Animals.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public void Remove(Animal entity)
    {
        context.Animals.Remove(entity);
        context.SaveChanges();
    }
}
