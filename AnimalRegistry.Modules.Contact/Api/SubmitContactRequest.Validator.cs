using AnimalRegistry.Modules.Contact.Domain;
using FastEndpoints;
using FluentValidation;
using System.Net.Mail;

namespace AnimalRegistry.Modules.Contact.Api;

internal sealed class SubmitContactRequestValidator : Validator<SubmitContactRequestBody>
{
    public SubmitContactRequestValidator()
    {
        RuleFor(x => x.ShelterName)
            .Must(NotBeBlank).WithMessage("Shelter name is required.")
            .Must(value => IsWithinLength(value, ContactRequest.ShelterNameMaxLength))
            .WithMessage($"Shelter name must be {ContactRequest.ShelterNameMaxLength} characters or fewer.");

        RuleFor(x => x.ContactPerson)
            .Must(NotBeBlank).WithMessage("Contact person is required.")
            .Must(value => IsWithinLength(value, ContactRequest.ContactPersonMaxLength))
            .WithMessage($"Contact person must be {ContactRequest.ContactPersonMaxLength} characters or fewer.");

        RuleFor(x => x.Email)
            .Must(NotBeBlank).WithMessage("E-mail is required.")
            .Must(value => IsWithinLength(value, ContactRequest.EmailMaxLength))
            .WithMessage($"E-mail must be {ContactRequest.EmailMaxLength} characters or fewer.")
            .Must(BeAValidEmail).WithMessage("E-mail is not a valid address.");

        RuleFor(x => x.Phone)
            .Must(value => IsWithinLength(value, ContactRequest.PhoneMaxLength))
            .WithMessage($"Phone must be {ContactRequest.PhoneMaxLength} characters or fewer.");

        RuleFor(x => x.Message)
            .Must(value => IsWithinLength(value, ContactRequest.MessageMaxLength))
            .WithMessage($"Message must be {ContactRequest.MessageMaxLength} characters or fewer.");

        RuleFor(x => x.Consent)
            .Equal(true).WithMessage("Consent is required.");
    }

    private static bool NotBeBlank(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool IsWithinLength(string? value, int maxLength)
    {
        return value is null || value.Trim().Length <= maxLength;
    }

    private static bool BeAValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();

        return MailAddress.TryCreate(trimmed, out var address) && address.Address == trimmed;
    }
}