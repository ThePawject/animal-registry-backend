using AnimalRegistry.Modules.Shelters.Domain;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Shelters.Infrastructure;

internal sealed class ShelterFeatureActivationRepository(SheltersDbContext context)
    : IShelterFeatureActivationRepository
{
    public async Task<IReadOnlyList<ShelterFeatureActivation>> GetByShelterIdAsync(string shelterId,
        CancellationToken cancellationToken = default)
    {
        return await context.ShelterFeatureActivations
            .Where(a => a.ShelterId == shelterId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ShelterFeatureActivation activation, CancellationToken cancellationToken = default)
    {
        await context.ShelterFeatureActivations.AddAsync(activation, cancellationToken);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            context.Entry(activation).State = EntityState.Detached;

            var exists = await context.ShelterFeatureActivations.AnyAsync(
                a => a.ShelterId == activation.ShelterId && a.FeatureKey == activation.FeatureKey,
                cancellationToken);
            if (!exists)
            {
                throw;
            }
        }
    }

    public async Task RemoveAsync(ShelterFeatureActivation activation, CancellationToken cancellationToken = default)
    {
        context.ShelterFeatureActivations.Remove(activation);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            context.Entry(activation).State = EntityState.Detached;
        }
    }
}
