using AnimalRegistry.Modules.Animals.Domain.Animals;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure;

internal sealed class AnimalsDbContext(DbContextOptions<AnimalsDbContext> options) : DbContext(options)
{
    public DbSet<Animal> Animals => Set<Animal>();
    public DbSet<AnimalPhoto> AnimalPhotos => Set<AnimalPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnimalsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}