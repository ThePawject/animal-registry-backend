using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Vaccinations;

internal sealed class AnimalVaccinationConfiguration : IEntityTypeConfiguration<AnimalVaccination>
{
    public void Configure(EntityTypeBuilder<AnimalVaccination> builder)
    {
        builder.ToTable("AnimalVaccinations");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.ShelterId).IsRequired().HasMaxLength(100);
        builder.Property(v => v.VaccineName).IsRequired().HasMaxLength(AnimalVaccination.VaccineNameMaxLength);
        builder.Property(v => v.AdministeredOn).IsRequired();
        builder.Property(v => v.Veterinarian).HasMaxLength(AnimalVaccination.VeterinarianMaxLength);
        builder.Property(v => v.BatchNumber).HasMaxLength(AnimalVaccination.BatchNumberMaxLength);
        builder.Property(v => v.Notes).HasMaxLength(AnimalVaccination.NotesMaxLength);
        builder.Property(v => v.CreatedBy).IsRequired().HasMaxLength(256);
        builder.Property(v => v.CreatedOn).IsRequired();
        builder.Property(v => v.ModifiedOn).IsRequired();

        builder.OwnsOne(v => v.Attachment, attachment =>
        {
            attachment.Property(a => a.BlobPath).HasColumnName("AttachmentBlobPath").IsRequired().HasMaxLength(500);
            attachment.Property(a => a.FileName).HasColumnName("AttachmentFileName").IsRequired().HasMaxLength(255);
            attachment.Property(a => a.ContentType).HasColumnName("AttachmentContentType").IsRequired()
                .HasMaxLength(100);
            attachment.Property(a => a.UploadedOn).HasColumnName("AttachmentUploadedOn").IsRequired();
        });
        builder.Navigation(v => v.Attachment).IsRequired(false);

        builder.HasIndex(v => new { v.ShelterId, v.AnimalId });
        builder.HasIndex(v => new { v.ShelterId, v.ValidUntil });
        builder.HasIndex(v => new { v.ShelterId, v.AdministeredOn });

        builder.HasOne<Animal>()
            .WithMany()
            .HasForeignKey(v => v.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
