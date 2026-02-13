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
        await context.SaveChangesAsync(cancellationToken);
        PopulatePhotoUrls(entity);
        return Result<Animal>.Success(entity);
    }

    public async Task RemoveAsync(Animal entity, CancellationToken cancellationToken = default)
    {
        context.Animals.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Animal>> GetAllByShelterIdAsync(string shelterId,
        CancellationToken cancellationToken = default)
    {
        var animals = await context.Animals
            .AsNoTracking()
            .Include(a => a.Photos)
            .Include(a => a.Events)
            .Include(a => a.HealthRecords)
            .Where(a => a.ShelterId == shelterId)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

        foreach (var animal in animals)
        {
            PopulatePhotoUrls(animal);
        }

        return animals;
    }

    public async Task<IReadOnlyList<Animal>> GetByIdsAsync(IEnumerable<Guid> ids, string shelterId,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();

        var animals = await context.Animals
            .AsNoTracking()
            .Include(a => a.Photos)
            .Include(a => a.Events)
            .Include(a => a.HealthRecords)
            .Where(a => a.ShelterId == shelterId && idList.Contains(a.Id))
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

        foreach (var animal in animals)
        {
            PopulatePhotoUrls(animal);
        }

        return animals;
    }

    public async Task<bool> IsSignatureUniqueAsync(string signature, string shelterId, Guid? excludeAnimalId = null,
        CancellationToken cancellationToken = default)
    {
        var animals = await context.Animals
            .Where(a => a.ShelterId == shelterId)
            .ToListAsync(cancellationToken);

        var query = animals.Where(a => a.Signature.Value == signature);

        if (excludeAnimalId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAnimalId.Value);
        }

        return !query.Any();
    }

    public async Task<IReadOnlyList<int>> GetExistingNumbersForYearAsync(int year, string shelterId,
        CancellationToken cancellationToken = default)
    {
        var prefix = $"{year:D4}/";

        var animals = await context.Animals
            .Where(a => a.ShelterId == shelterId)
            .ToListAsync(cancellationToken);

        var numbers = new List<int>();
        foreach (var animal in animals)
        {
            if (animal.Signature.Value.StartsWith(prefix))
            {
                var parts = animal.Signature.Value.Split('/');
                if (parts.Length == 2 && int.TryParse(parts[1], out var number))
                {
                    numbers.Add(number);
                }
            }
        }

        return numbers.AsReadOnly();
    }

    public async Task<PagedResult<Animal>> ListAsync(string shelterId, int page, int pageSize, string? keyWordSearch,
        CancellationToken cancellationToken = default)
    {
        var query = context.Animals.Where(a => a.ShelterId == shelterId);

        if (!string.IsNullOrWhiteSpace(keyWordSearch))
        {
            var terms = keyWordSearch
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(term => term.ToLower())
                .ToArray();

            var animals = await query.ToListAsync(cancellationToken);
            var filteredAnimals = animals.Where(a =>
            {
                var searchableText = $"{a.Signature.Value} {a.TransponderCode} {a.Name} {a.Color} {a.ShelterId} " +
                                     string.Join(" ", a.Events.Select(e => $"{e.Description} {e.PerformedBy}"));
                return terms.All(term => searchableText.Contains(term, StringComparison.OrdinalIgnoreCase));
            }).ToList();

            var filteredTotalCount = filteredAnimals.Count;
            var filteredItems = filteredAnimals
                .OrderByDescending(x => x.ModifiedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var animal in filteredItems)
            {
                PopulatePhotoUrls(animal);
            }

            return new PagedResult<Animal>(filteredItems, filteredTotalCount, page, pageSize);
        }

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