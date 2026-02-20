using AnimalRegistry.Shared.Outbox.Domain;

namespace AnimalRegistry.Shared.Outbox.Application;

/// <summary>
///     Module-specific repository interface for OutboxMessages.
///     Each module that uses Outbox pattern should implement this interface.
/// </summary>
public interface IModuleOutboxMessageRepository
{
    Task<List<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken = default);

    Task<List<OutboxMessage>> GetFailedMessagesReadyForRetryAsync(int batchSize, int maxRetryCount,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}