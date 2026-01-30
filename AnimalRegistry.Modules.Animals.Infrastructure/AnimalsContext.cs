using AnimalRegistry.Modules.Animals.Domain.Animals;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure;

public class AnimalsContext(DbContextOptions<AnimalsContext> options) : DbContext(options)
{
    private const string Schema = "animals";

    public DbSet<Animal> Animals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
    }
}