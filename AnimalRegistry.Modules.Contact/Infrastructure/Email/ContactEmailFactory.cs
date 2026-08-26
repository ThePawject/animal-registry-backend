using AnimalRegistry.Modules.Contact.Domain;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace AnimalRegistry.Modules.Contact.Infrastructure.Email;

public interface IContactEmailFactory
{
    EmailMessage Create(ContactRequest contactRequest);
}

internal sealed class ContactEmailFactory(IOptions<EmailSettings> settings) : IContactEmailFactory
{
    private const string SubjectPrefix = "[MojeSchronisko] Inquiry: ";
    private const int SubjectMaxLength = 200;

    private readonly EmailSettings _settings = settings.Value;

    public EmailMessage Create(ContactRequest contactRequest)
    {
        ArgumentNullException.ThrowIfNull(contactRequest);

        var shelterName = EmailHeaderSanitizer.Sanitize(
            contactRequest.ShelterName,
            SubjectMaxLength - SubjectPrefix.Length);

        var replyTo = TryGetReplyToAddress(contactRequest.Email);

        return new EmailMessage(
            _settings.ContactRecipient,
            SubjectPrefix + shelterName,
            BuildHtmlBody(contactRequest),
            BuildTextBody(contactRequest),
            replyTo,
            replyTo is null ? null : EmailHeaderSanitizer.Sanitize(contactRequest.ContactPerson));
    }

    private static string? TryGetReplyToAddress(string email)
    {
        var sanitized = EmailHeaderSanitizer.Sanitize(email, ContactRequest.EmailMaxLength);

        return MailAddress.TryCreate(sanitized, out var address) ? address.Address : null;
    }

    private static string BuildHtmlBody(ContactRequest contactRequest)
    {
        var builder = new StringBuilder();
        builder.Append("<!DOCTYPE html><html lang=\"en\"><body style=\"font-family:sans-serif;font-size:14px\">");
        builder.Append("<h2>New contact form submission</h2><table cellpadding=\"4\">");

        AppendHtmlRow(builder, "Shelter", contactRequest.ShelterName);
        AppendHtmlRow(builder, "Contact person", contactRequest.ContactPerson);
        AppendHtmlRow(builder, "E-mail", contactRequest.Email);
        AppendHtmlRow(builder, "Phone", contactRequest.Phone);
        AppendHtmlRow(builder, "Consent given", contactRequest.ConsentGivenOn.ToString("u"));
        AppendHtmlRow(builder, "Reference", contactRequest.Id.ToString());

        builder.Append("</table>");

        if (!string.IsNullOrWhiteSpace(contactRequest.Message))
        {
            builder.Append("<h3>Message</h3><p>");
            builder.Append(WebUtility.HtmlEncode(contactRequest.Message).ReplaceLineEndings("<br>"));
            builder.Append("</p>");
        }

        builder.Append("</body></html>");

        return builder.ToString();
    }

    private static string BuildTextBody(ContactRequest contactRequest)
    {
        var builder = new StringBuilder();
        builder.AppendLine("New contact form submission");
        builder.AppendLine();
        builder.AppendLine($"Shelter: {contactRequest.ShelterName}");
        builder.AppendLine($"Contact person: {contactRequest.ContactPerson}");
        builder.AppendLine($"E-mail: {contactRequest.Email}");
        builder.AppendLine($"Phone: {contactRequest.Phone ?? "-"}");
        builder.AppendLine($"Consent given: {contactRequest.ConsentGivenOn:u}");
        builder.AppendLine($"Reference: {contactRequest.Id}");

        if (!string.IsNullOrWhiteSpace(contactRequest.Message))
        {
            builder.AppendLine();
            builder.AppendLine("Message:");
            builder.AppendLine(contactRequest.Message);
        }

        return builder.ToString();
    }

    private static void AppendHtmlRow(StringBuilder builder, string label, string? value)
    {
        builder.Append("<tr><td><strong>");
        builder.Append(WebUtility.HtmlEncode(label));
        builder.Append("</strong></td><td>");
        builder.Append(WebUtility.HtmlEncode(value ?? "-"));
        builder.Append("</td></tr>");
    }
}