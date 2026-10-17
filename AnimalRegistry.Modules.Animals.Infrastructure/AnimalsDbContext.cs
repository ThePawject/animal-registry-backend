using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalHealths;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared.DDD;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure;

internal sealed class AnimalsDbContext(DbContextOptions<AnimalsDbContext> options, IDomainEventDispatcher dispatcher)
    : DomainEventBaseDbContext(options, dispatcher)

{
    public DbSet<Animal> Animals => Set<Animal>();
    public DbSet<AnimalEvent> AnimalEvents => Set<AnimalEvent>();
    public DbSet<AnimalHealth> AnimalHealthRecords => Set<AnimalHealth>();
    public DbSet<AnimalPhoto> AnimalPhotos => Set<AnimalPhoto>();
    public DbSet<AnimalHealthDocument> AnimalHealthDocuments => Set<AnimalHealthDocument>();
    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<AnimalOrigin> AnimalOrigins => Set<AnimalOrigin>();
    public DbSet<AnimalVaccination> AnimalVaccinations => Set<AnimalVaccination>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnimalsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}