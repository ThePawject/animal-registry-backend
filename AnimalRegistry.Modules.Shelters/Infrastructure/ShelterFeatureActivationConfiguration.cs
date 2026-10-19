using AnimalRegistry.Modules.Shelters.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalRegistry.Modules.Shelters.Infrastructure;

internal sealed class ShelterFeatureActivationConfiguration : IEntityTypeConfiguration<ShelterFeatureActivation>
{
    public void Configure(EntityTypeBuilder<ShelterFeatureActivation> builder)
    {
        builder.ToTable("ShelterFeatureActivations");
        builder.HasKey(a => new { a.ShelterId, a.FeatureKey });

        builder.Property(a => a.ShelterId).IsRequired().HasMaxLength(100);
        builder.Property(a => a.FeatureKey).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnabledBy).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnabledOn).IsRequired();
    }
}
