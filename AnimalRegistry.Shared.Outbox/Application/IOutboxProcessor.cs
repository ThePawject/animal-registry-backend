using AnimalRegistry.Shared.Outbox.Domain;

namespace AnimalRegistry.Shared.Outbox.Application;

public interface IOutboxProcessor
{
    Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}