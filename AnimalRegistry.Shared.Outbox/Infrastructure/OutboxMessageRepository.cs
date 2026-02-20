using AnimalRegistry.Shared.Outbox.Application;
using AnimalRegistry.Shared.Outbox.Domain;

namespace AnimalRegistry.Shared.Outbox.Infrastructure;

/// <summary>
///     Aggregating repository that collects OutboxMessages from all registered module-specific repositories
/// </summary>
public class OutboxMessageRepository(IEnumerable<IModuleOutboxMessageRepository> moduleRepositories)
    : IOutboxMessageRepository
{
    public async Task<List<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var allPendingMessages = new List<OutboxMessage>();

        foreach (var repository in moduleRepositories)
        {
            var messages = await repository.GetPendingMessagesAsync(batchSize, cancellationToken);
            allPendingMessages.AddRange(messages);
        }

        return allPendingMessages
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToList();
    }

    public async Task<List<OutboxMessage>> GetFailedMessagesReadyForRetryAsync(
        int batchSize,
        int maxRetryCount,
        CancellationToken cancellationToken = default)
    {
        var allRetryableMessages = new List<OutboxMessage>();

        foreach (var repository in moduleRepositories)
        {
            var messages =
                await repository.GetFailedMessagesReadyForRetryAsync(batchSize, maxRetryCount, cancellationToken);
            allRetryableMessages.AddRange(messages);
        }

        return allRetryableMessages
            .OrderBy(x => x.NextRetryAt)
            .Take(batchSize)
            .ToList();
    }

    public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task UpdateRangeAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var repository in moduleRepositories)
        {
            await repository.SaveChangesAsync(cancellationToken);
        }
    }
}