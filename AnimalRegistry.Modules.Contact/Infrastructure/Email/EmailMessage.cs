namespace AnimalRegistry.Modules.Contact.Infrastructure.Email;

public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string TextBody,
    string? ReplyTo,
    string? ReplyToDisplayName);