using AnimalRegistry.Shared.Outbox.Domain;

namespace AnimalRegistry.Shared.Outbox.Application;

public interface IOutboxMessageRepository
{
    Task<List<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken = default);

    Task<List<OutboxMessage>> GetFailedMessagesReadyForRetryAsync(int batchSize, int maxRetryCount,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}