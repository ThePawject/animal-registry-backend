using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Origin;

internal sealed class AnimalOriginRepository(AnimalsDbContext context) : IAnimalOriginRepository
{
    public Task<AnimalOrigin?> GetByAnimalIdAsync(Guid animalId, string shelterId,
        CancellationToken cancellationToken = default)
    {
        return context.AnimalOrigins
            .FirstOrDefaultAsync(o => o.AnimalId == animalId && o.ShelterId == shelterId, cancellationToken);
    }

    public async Task<IReadOnlyList<AnimalOrigin>> GetByAnimalIdsAsync(IEnumerable<Guid> animalIds, string shelterId,
        CancellationToken cancellationToken = default)
    {
        var idList = animalIds.Distinct().ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        return await context.AnimalOrigins
            .AsNoTracking()
            .Where(o => o.ShelterId == shelterId && idList.Contains(o.AnimalId))
            .ToListAsync(cancellationToken);
    }

    public void Add(AnimalOrigin origin)
    {
        context.AnimalOrigins.Add(origin);
    }
}
