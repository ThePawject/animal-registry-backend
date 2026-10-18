using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Origin;

internal static class AnimalOriginConfiguration
{
    public const string PrimaryKeyName = "PK_AnimalOrigins";

    public static void Configure(OwnedNavigationBuilder<Animal, AnimalOrigin> builder)
    {
        builder.ToTable("AnimalOrigins");
        builder.WithOwner().HasForeignKey("AnimalId");
        builder.HasKey("AnimalId").HasName(PrimaryKeyName);

        builder.Property(o => o.AdmissionDate).IsRequired();
        builder.Property(o => o.AdmissionMethod)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();
        builder.Property(o => o.FoundStreet).IsRequired().HasMaxLength(200);
        builder.Property(o => o.FoundMunicipalityId).IsRequired();
        builder.Property(o => o.ContractorMunicipalityId).IsRequired();

        builder.HasIndex(o => o.AdmissionDate);

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
