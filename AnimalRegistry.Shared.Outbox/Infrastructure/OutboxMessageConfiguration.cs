using AnimalRegistry.Shared.Outbox.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalRegistry.Shared.Outbox.Infrastructure;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MessageType)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.MessageData)
            .IsRequired();

        builder.Property(x => x.ModuleName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ProcessedAt);

        builder.Property(x => x.RetryCount)
            .IsRequired();

        builder.Property(x => x.NextRetryAt);

        builder.Property(x => x.Error);

        builder.HasIndex(x => new { x.Status, x.CreatedAt })
            .HasDatabaseName("IX_OutboxMessages_Status_CreatedAt");

        builder.HasIndex(x => new { x.Status, x.NextRetryAt })
            .HasDatabaseName("IX_OutboxMessages_Status_NextRetryAt");

        builder.HasIndex(x => x.ModuleName)
            .HasDatabaseName("IX_OutboxMessages_ModuleName");

        builder.Ignore(x => x.DomainEvents);
    }
}