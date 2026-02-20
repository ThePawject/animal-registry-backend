using AnimalRegistry.Shared.Outbox.Application;
using AnimalRegistry.Shared.Outbox.Domain;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Persistence;

internal sealed class AnimalsOutboxMessageRepository(AnimalsDbContext context) : IModuleOutboxMessageRepository
{
    public async Task<List<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await context.OutboxMessages
            .Where(x => x.Status == OutboxMessageStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OutboxMessage>> GetFailedMessagesReadyForRetryAsync(
        int batchSize,
        int maxRetryCount,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await context.OutboxMessages
            .Where(x => x.Status == OutboxMessageStatus.Failed &&
                        x.RetryCount < maxRetryCount &&
                        (x.NextRetryAt == null || x.NextRetryAt <= now))
            .OrderBy(x => x.NextRetryAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}