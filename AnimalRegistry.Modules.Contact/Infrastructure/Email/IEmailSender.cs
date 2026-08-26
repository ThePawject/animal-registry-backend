namespace AnimalRegistry.Modules.Contact.Infrastructure.Email;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}