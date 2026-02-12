using AnimalRegistry.Modules.Animals.Domain.Animals;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals;

internal sealed class AnimalEventRepository(AnimalsDbContext context) : IAnimalEventRepository
{
    public async Task<IReadOnlyList<AnimalEventWithAnimalInfo>> GetAllByShelterIdAsync(string shelterId,
        CancellationToken cancellationToken = default)
    {
        var events = await context.Animals
            .AsNoTracking()
            .Where(a => a.ShelterId == shelterId)
            .SelectMany(a => a.Events.Select(e => new AnimalEventWithAnimalInfo(e, a.Species, a.Id, a.Name)))
            .ToListAsync(cancellationToken);

        return events;
    }

    public async Task<IReadOnlyList<AnimalEventWithAnimalInfo>> GetByDateRangeAsync(
        string shelterId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken = default)
    {
        var events = await context.Animals
            .AsNoTracking()
            .Where(a => a.ShelterId == shelterId)
            .SelectMany(a => a.Events
                .Where(e => e.OccurredOn >= startDate && e.OccurredOn <= endDate)
                .Select(e => new AnimalEventWithAnimalInfo(e, a.Species, a.Id, a.Name)))
            .ToListAsync(cancellationToken);

        return events;
    }
}