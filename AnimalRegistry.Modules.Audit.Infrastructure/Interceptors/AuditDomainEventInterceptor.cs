using AnimalRegistry.Modules.Audit.Application.Services;
using AnimalRegistry.Shared.Auditing;
using AnimalRegistry.Shared.DDD;
using Microsoft.Extensions.Logging;

namespace AnimalRegistry.Modules.Audit.Infrastructure.Interceptors;

public class AuditDomainEventInterceptor(
    IDomainEventDispatcher innerDispatcher,
    IAuditService auditService,
    ILogger<AuditDomainEventInterceptor> logger)
    : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        var eventsList = domainEvents.ToList();

        foreach (var domainEvent in eventsList)
        {
            if (ShouldAudit(domainEvent))
            {
                try
                {
                    await auditService.AuditDomainEventAsync(domainEvent, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to audit domain event {EventType}", domainEvent.GetType().Name);
                }
            }
        }

        await innerDispatcher.DispatchAsync(eventsList, cancellationToken);
    }

    private static bool ShouldAudit(IDomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();
        return Attribute.IsDefined(eventType, typeof(AuditableAttribute));
    }
}