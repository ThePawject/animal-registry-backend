using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Origin;

internal sealed class AnimalOriginConfiguration : IEntityTypeConfiguration<AnimalOrigin>
{
    public void Configure(EntityTypeBuilder<AnimalOrigin> builder)
    {
        builder.ToTable("AnimalOrigins");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.ShelterId).IsRequired().HasMaxLength(100);
        builder.Property(o => o.AdmissionDate).IsRequired();
        builder.Property(o => o.AdmissionMethod)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();
        builder.Property(o => o.FoundStreet).IsRequired().HasMaxLength(200);
        builder.Property(o => o.FoundMunicipalityId).IsRequired();
        builder.Property(o => o.ContractorMunicipalityId).IsRequired();

        builder.HasIndex(o => o.AnimalId).IsUnique();
        builder.HasIndex(o => new { o.ShelterId, o.AdmissionDate });

        builder.HasOne<Animal>()
            .WithMany()
            .HasForeignKey(o => o.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Municipality>()
            .WithMany()
            .HasForeignKey(o => o.FoundMunicipalityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Municipality>()
            .WithMany()
            .HasForeignKey(o => o.ContractorMunicipalityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
