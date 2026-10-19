using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Municipalities;

internal sealed class MunicipalityConfiguration : IEntityTypeConfiguration<Municipality>
{
    public void Configure(EntityTypeBuilder<Municipality> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ShelterId).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(Municipality.NameMaxLength);
        builder.Property(m => m.IsArchived).IsRequired();
        builder.Property(m => m.CreatedOn).IsRequired();

        builder.HasIndex(m => new { m.ShelterId, m.Name })
            .IsUnique()
            .HasDatabaseName("IX_Municipalities_ShelterId_Name");
    }
}
