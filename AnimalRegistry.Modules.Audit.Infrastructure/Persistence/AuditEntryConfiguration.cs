using AnimalRegistry.Modules.Audit.Domain.AuditEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalRegistry.Modules.Audit.Infrastructure.Persistence;

internal sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.EntityData)
            .IsRequired();

        builder.Property(x => x.Timestamp)
            .IsRequired();

        builder.Property(x => x.ExecutionTime);

        builder.Property(x => x.IsSuccess)
            .IsRequired();

        builder.Property(x => x.ErrorMessage);

        builder.OwnsOne(x => x.Metadata, metadataBuilder =>
        {
            metadataBuilder.Property(m => m.UserId)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("UserId");

            metadataBuilder.Property(m => m.Email)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("Email");

            metadataBuilder.Property(m => m.ShelterId)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("ShelterId");

            metadataBuilder.Property(m => m.IpAddress)
                .HasMaxLength(45) // IPv6 max length
                .HasColumnName("IpAddress");

            metadataBuilder.Property(m => m.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("UserAgent");
        });

        builder.HasIndex(x => x.Timestamp)
            .HasDatabaseName("IX_AuditEntries_Timestamp");

        builder.HasIndex(x => x.Type)
            .HasDatabaseName("IX_AuditEntries_Type");

        builder.HasIndex(x => new { x.Type, x.Timestamp })
            .HasDatabaseName("IX_AuditEntries_Type_Timestamp");

        builder.HasIndex("Metadata_UserId")
            .HasDatabaseName("IX_AuditEntries_UserId");

        builder.Ignore(x => x.DomainEvents);
    }
}