using AnimalRegistry.Shared.DDD;
using AnimalRegistry.Shared.Outbox.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace AnimalRegistry.Shared.Outbox.Infrastructure;

public class OutboxInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false, PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        ConvertDomainEventsToOutboxMessages(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ConvertDomainEventsToOutboxMessages(DbContext context)
    {
        var moduleName = GetModuleName(context.GetType());

        var domainEntities = context.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        if (!domainEntities.Any())
        {
            return;
        }

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            var messageType = domainEvent.GetType().AssemblyQualifiedName
                              ?? domainEvent.GetType().FullName
                              ?? domainEvent.GetType().Name;

            var messageData = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions);

            var outboxMessage = OutboxMessage.Create(
                messageType,
                messageData,
                moduleName);

            context.Add(outboxMessage);
        }

        foreach (var entity in domainEntities)
        {
            entity.ClearDomainEvents();
        }
    }

    private static string GetModuleName(Type dbContextType)
    {
        var contextName = dbContextType.Name;

        if (contextName.EndsWith("DbContext"))
        {
            contextName = contextName[..^"DbContext".Length];
        }

        return contextName;
    }
}