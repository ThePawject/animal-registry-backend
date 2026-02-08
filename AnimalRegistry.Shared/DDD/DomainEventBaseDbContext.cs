using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Shared.DDD;

public abstract class DomainEventBaseDbContext(DbContextOptions options, IDomainEventDispatcher dispatcher)
    : DbContext(options)
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        await PublishDomainEvents(cancellationToken);
        return result;
    }

    private async Task PublishDomainEvents(CancellationToken cancellationToken)
    {
        var domainEntities = ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        domainEntities.ForEach(entity => entity.ClearDomainEvents());

        if (domainEvents.Count == 0)
        {
            return;
        }

        await dispatcher.DispatchAsync(domainEvents, cancellationToken);
    }
}