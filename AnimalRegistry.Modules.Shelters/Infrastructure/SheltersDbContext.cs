using AnimalRegistry.Modules.Shelters.Domain;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Shelters.Infrastructure;

internal sealed class SheltersDbContext(DbContextOptions<SheltersDbContext> options) : DbContext(options)
{
    public const string MigrationsHistoryTable = "__EFMigrationsHistoryShelters";

    public DbSet<ShelterFeatureActivation> ShelterFeatureActivations => Set<ShelterFeatureActivation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SheltersDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
