using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalHealths;
using AnimalRegistry.Shared.DDD;
using AnimalRegistry.Shared.Outbox.Domain;
using AnimalRegistry.Shared.Outbox.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure;

internal sealed class AnimalsDbContext(DbContextOptions<AnimalsDbContext> options, IDomainEventDispatcher dispatcher)
    : DomainEventBaseDbContext(options, dispatcher)

{
    public DbSet<Animal> Animals => Set<Animal>();
    public DbSet<AnimalEvent> AnimalEvents => Set<AnimalEvent>();
    public DbSet<AnimalHealth> AnimalHealthRecords => Set<AnimalHealth>();
    public DbSet<AnimalPhoto> AnimalPhotos => Set<AnimalPhoto>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnimalsDbContext).Assembly);
        modelBuilder.ApplyOutboxConfiguration();
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddOutboxInterceptor();
        base.OnConfiguring(optionsBuilder);
    }
}