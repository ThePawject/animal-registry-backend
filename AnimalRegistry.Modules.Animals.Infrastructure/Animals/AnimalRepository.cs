using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals;

internal sealed class AnimalRepository(AnimalsDbContext context) : IAnimalRepository
{
    public async Task<Animal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Animals.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Result<Animal>> AddAsync(Animal entity, CancellationToken cancellationToken = default)
    {
        var entityEntry = await context.Animals.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Result<Animal>.Success(entityEntry.Entity);
    }

    public void Remove(Animal entity)
    {
        context.Animals.Remove(entity);
        context.SaveChanges();
    }

    public async Task<IEnumerable<Animal>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await context.Animals.ToListAsync(cancellationToken);
    }
}