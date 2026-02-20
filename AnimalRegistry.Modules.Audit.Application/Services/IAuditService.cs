using AnimalRegistry.Shared.DDD;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Audit.Application.Services;

public interface IAuditService
{
    Task AuditDomainEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    Task AuditCommandAsync<TResponse>(
        IRequest<TResponse> command,
        TResponse result,
        TimeSpan executionTime,
        CancellationToken cancellationToken = default);
}