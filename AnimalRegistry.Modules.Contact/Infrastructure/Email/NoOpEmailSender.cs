using Microsoft.Extensions.Logging;

namespace AnimalRegistry.Modules.Contact.Infrastructure.Email;

internal sealed class NoOpEmailSender(ILogger<NoOpEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        logger.LogWarning(
            "E-mail sending is disabled - the contact notification was not sent. Subject: {Subject}.",
            message.Subject);

        return Task.CompletedTask;
    }
}