namespace AnimalRegistry.Modules.Contact.Domain;

public interface IContactRequestRepository
{
    Task AddAsync(ContactRequest contactRequest, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}