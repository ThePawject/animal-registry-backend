using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace AnimalRegistry.Modules.Contact.Infrastructure.Email;

internal sealed class EmailSettingsValidator : IValidateOptions<EmailSettings>
{
    private const int BlockedSmtpPort = 25;

    public ValidateOptionsResult Validate(string? name, EmailSettings options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            failures.Add($"{EmailSettings.SectionName}:Host is required when e-mail sending is enabled.");
        }

        if (options.Port is <= 0 or > 65535)
        {
            failures.Add($"{EmailSettings.SectionName}:Port must be a valid TCP port.");
        }
        else if (options.Port == BlockedSmtpPort)
        {
            failures.Add(
                $"{EmailSettings.SectionName}:Port 25 is blocked for outbound traffic on Azure App Service. " +
                "Use the submission port 587 (STARTTLS), 465 (implicit TLS) or a provider HTTP API.");
        }

        if (options.TimeoutSeconds <= 0)
        {
            failures.Add($"{EmailSettings.SectionName}:TimeoutSeconds must be greater than zero.");
        }

        if (!IsValidAddress(options.FromAddress))
        {
            failures.Add($"{EmailSettings.SectionName}:FromAddress must be a valid e-mail address.");
        }

        if (!IsValidAddress(options.ContactRecipient))
        {
            failures.Add($"{EmailSettings.SectionName}:ContactRecipient must be a valid e-mail address.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsValidAddress(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && MailAddress.TryCreate(value, out _);
    }
}