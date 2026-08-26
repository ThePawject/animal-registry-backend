using AnimalRegistry.Modules.Contact.Domain;

namespace AnimalRegistry.Modules.Contact.Infrastructure;

internal sealed class ContactRequestRepository(ContactDbContext dbContext) : IContactRequestRepository
{
    public async Task AddAsync(ContactRequest contactRequest, CancellationToken cancellationToken = default)
    {
        await dbContext.ContactRequests.AddAsync(contactRequest, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}