using AnimalRegistry.Modules.Audit.Domain.AuditEntries;
using AnimalRegistry.Shared.DDD;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Audit.Infrastructure.Persistence;

public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options, IDomainEventDispatcher dispatcher)
    : DomainEventBaseDbContext(options, dispatcher)
{
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}