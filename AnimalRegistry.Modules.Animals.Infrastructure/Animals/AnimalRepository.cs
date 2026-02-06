using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals;

internal sealed class AnimalRepository(AnimalsDbContext context) : IAnimalRepository
{
    public async Task<Animal?> GetByIdAsync(Guid id, string shelterId, CancellationToken cancellationToken = default)
    {
        return await context.Animals.FirstOrDefaultAsync(a => a.Id == id && a.ShelterId == shelterId,
            cancellationToken);
    }

    public async Task<Result<Animal>> AddAsync(Animal entity, CancellationToken cancellationToken = default)
    {
        var entityEntry = await context.Animals.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Result<Animal>.Success(entityEntry.Entity);
    }
    
    public async Task<Result<Animal>> UpdateAsync(Animal entity, CancellationToken cancellationToken = default)
    {
        var entityEntry = context.Animals.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
        return Result<Animal>.Success(entityEntry.Entity);
    }

    public void Remove(Animal entity)
    {
        context.Animals.Remove(entity);
        context.SaveChanges();
    }

    public async Task<PagedResult<Animal>> ListAsync(string shelterId, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Animals.Where(a => a.ShelterId == shelterId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Animal>(items, totalCount, page, pageSize);
    }
}