using AnimalRegistry.Modules.Contact.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalRegistry.Modules.Contact.Infrastructure;

internal sealed class ContactRequestConfiguration : IEntityTypeConfiguration<ContactRequest>
{
    public void Configure(EntityTypeBuilder<ContactRequest> builder)
    {
        builder.ToTable("ContactRequests");
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.ShelterName).IsRequired().HasMaxLength(ContactRequest.ShelterNameMaxLength);
        builder.Property(x => x.ContactPerson).IsRequired().HasMaxLength(ContactRequest.ContactPersonMaxLength);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(ContactRequest.EmailMaxLength);
        builder.Property(x => x.Phone).HasMaxLength(ContactRequest.PhoneMaxLength);
        builder.Property(x => x.Message).HasMaxLength(ContactRequest.MessageMaxLength);
        builder.Property(x => x.ConsentGivenOn).IsRequired();
        builder.Property(x => x.CreatedOn).IsRequired();
        builder.Property(x => x.DeliveryStatus).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.DeliveryAttempts).IsRequired();
        builder.Property(x => x.DeliveryError).HasMaxLength(ContactRequest.DeliveryErrorMaxLength);

        builder.HasIndex(x => new { x.DeliveryStatus, x.CreatedOn })
            .HasDatabaseName("IX_ContactRequests_DeliveryStatus_CreatedOn");
    }
}