using AnimalRegistry.Modules.Animals.Domain.Animals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals;

internal sealed class AnimalConfiguration : IEntityTypeConfiguration<Animal>
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
        builder.Property(a => a.ShelterId).IsRequired().HasMaxLength(100);
        builder.HasIndex(a => a.ShelterId);

        builder.OwnsMany(a => a.Photos, photoBuilder =>
        {
            photoBuilder.HasKey(p => p.Id);
            photoBuilder.Property(p => p.BlobUrl).IsRequired().HasMaxLength(500);
            photoBuilder.Property(p => p.FileName).IsRequired().HasMaxLength(255);
            photoBuilder.Property(p => p.IsMain).IsRequired();
            photoBuilder.Property(p => p.UploadedOn).IsRequired();
            photoBuilder.HasIndex(p => p.IsMain);
        });

        builder.OwnsMany(a => a.Events, eventBuilder =>
        {
            eventBuilder.Property(e => e.Type)
                .IsRequired()
                .HasConversion<string>();
            eventBuilder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);
            eventBuilder.Property(e => e.PerformedBy)
                .IsRequired()
                .HasMaxLength(100);
            eventBuilder.Property(e => e.OccurredOn)
                .IsRequired();

            // eventBuilder.WithOwner().HasForeignKey("AnimalId");
        });
    }
}