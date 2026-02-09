using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals;

internal sealed class AnimalRepository(
    AnimalsDbContext context,
    IBlobStorageService blobStorageService) : IAnimalRepository
{
    public async Task<Animal?> GetByIdAsync(Guid id, string shelterId, CancellationToken cancellationToken = default)
    {
        var animal = await context.Animals
            .Include(a => a.Photos)
            .Include(a => a.Events)
            .FirstOrDefaultAsync(a => a.Id == id && a.ShelterId == shelterId, cancellationToken);
        
        if (animal is not null)
        {
            PopulatePhotoUrls(animal);
        }
        
        return animal;
    }

    public async Task<Result<Animal>> AddAsync(Animal entity, CancellationToken cancellationToken = default)
    {
        var entityEntry = await context.Animals.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        PopulatePhotoUrls(entityEntry.Entity);
        return Result<Animal>.Success(entityEntry.Entity);
    }
    
    public async Task<Result<Animal>> UpdateAsync(Animal entity, CancellationToken cancellationToken = default)
    {
        var entityEntry = context.Animals.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
        PopulatePhotoUrls(entityEntry.Entity);
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
            .OrderByDescending(x => x.ModifiedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        foreach (var animal in items)
        {
            PopulatePhotoUrls(animal);
        }

        return new PagedResult<Animal>(items, totalCount, page, pageSize);
    }
    
    private void PopulatePhotoUrls(Animal animal)
    {
        foreach (var photo in animal.Photos)
        {
            photo.Url = blobStorageService.GetBlobUrl(photo.BlobPath);
        }
    }
}
