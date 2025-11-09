using AnimalRegistry.Modules.Animals.Domain.Animals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals;

public class AnimalConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Signature).IsRequired().HasMaxLength(100);
        builder.Property(a => a.TransponderCode).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Color).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Species).IsRequired();
        builder.Property(a => a.Sex).IsRequired();
        builder.Property(a => a.BirthDate).IsRequired();
        builder.Property(a => a.CreatedOn).IsRequired();
        builder.Property(a => a.ModifiedOn).IsRequired();
        builder.Property(a => a.IsActive).IsRequired();
    }
}
