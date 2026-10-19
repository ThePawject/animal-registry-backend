using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Municipalities;

internal sealed class MunicipalityRepository(AnimalsDbContext context) : IMunicipalityRepository
{
    public Task<Municipality?> GetByIdAsync(Guid id, string shelterId, CancellationToken cancellationToken = default)
    {
        return context.Municipalities
            .FirstOrDefaultAsync(m => m.Id == id && m.ShelterId == shelterId, cancellationToken);
    }

    public async Task<IReadOnlyList<Municipality>> ListAsync(string shelterId, bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        return await context.Municipalities
            .AsNoTracking()
            .Where(m => m.ShelterId == shelterId && (includeArchived || !m.IsArchived))
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Municipality>> GetByIdsAsync(IEnumerable<Guid> ids, string shelterId,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        return await context.Municipalities
            .AsNoTracking()
            .Where(m => m.ShelterId == shelterId && idList.Contains(m.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(string name, string shelterId, Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var trimmed = name.Trim();
        return !await context.Municipalities
            .AnyAsync(m => m.ShelterId == shelterId && m.Name == trimmed && m.Id != excludeId, cancellationToken);
    }

    public async Task<bool> AddAsync(Municipality municipality, CancellationToken cancellationToken = default)
    {
        await context.Municipalities.AddAsync(municipality, cancellationToken);
        return await SaveWithUniqueNameAsync(municipality, cancellationToken);
    }

    public async Task<bool> UpdateAsync(Municipality municipality, CancellationToken cancellationToken = default)
    {
        context.Municipalities.Update(municipality);
        return await SaveWithUniqueNameAsync(municipality, cancellationToken);
    }

    private async Task<bool> SaveWithUniqueNameAsync(Municipality municipality, CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            context.Entry(municipality).State = EntityState.Detached;
            if (await IsNameUniqueAsync(municipality.Name, municipality.ShelterId, municipality.Id, cancellationToken))
            {
                throw;
            }

            return false;
        }
    }
}
