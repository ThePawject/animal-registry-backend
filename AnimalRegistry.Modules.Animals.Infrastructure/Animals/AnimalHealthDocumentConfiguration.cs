using AnimalRegistry.Modules.Animals.Domain.Animals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals;

internal sealed class AnimalHealthDocumentConfiguration : IEntityTypeConfiguration<AnimalHealthDocument>
{
    public void Configure(EntityTypeBuilder<AnimalHealthDocument> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.HealthRecordId)
            .IsRequired();

        builder.Property(d => d.BlobPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.UploadedOn)
            .IsRequired();

        builder.HasIndex(d => d.HealthRecordId)
            .HasDatabaseName("IX_AnimalHealthDocuments_HealthRecordId");
    }
}
