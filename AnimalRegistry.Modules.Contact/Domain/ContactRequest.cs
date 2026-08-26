using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Contact.Domain;

public sealed class ContactRequest : Entity
{
    public const int ShelterNameMaxLength = 200;
    public const int ContactPersonMaxLength = 200;
    public const int EmailMaxLength = 254;
    public const int PhoneMaxLength = 32;
    public const int MessageMaxLength = 4000;
    public const int DeliveryErrorMaxLength = 1000;

    private ContactRequest()
    {
    }

    private ContactRequest(
        string shelterName,
        string contactPerson,
        string email,
        string? phone,
        string? message,
        DateTimeOffset consentGivenOn)
    {
        ShelterName = shelterName;
        ContactPerson = contactPerson;
        Email = email;
        Phone = phone;
        Message = message;
        ConsentGivenOn = consentGivenOn;
        CreatedOn = consentGivenOn;
        DeliveryStatus = ContactRequestDeliveryStatus.Pending;
    }

    public string ShelterName { get; private set; } = null!;
    public string ContactPerson { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? Phone { get; private set; }
    public string? Message { get; private set; }

    public DateTimeOffset ConsentGivenOn { get; private set; }

    public DateTimeOffset CreatedOn { get; private set; }
    public ContactRequestDeliveryStatus DeliveryStatus { get; private set; }
    public DateTimeOffset? DeliveryCompletedOn { get; private set; }
    public int DeliveryAttempts { get; private set; }
    public string? DeliveryError { get; private set; }

    public static ContactRequest Create(
        string shelterName,
        string contactPerson,
        string email,
        string? phone,
        string? message,
        DateTimeOffset consentGivenOn)
    {
        return new ContactRequest(
            NormalizeRequired(shelterName, ShelterNameMaxLength, nameof(shelterName)),
            NormalizeRequired(contactPerson, ContactPersonMaxLength, nameof(contactPerson)),
            NormalizeRequired(email, EmailMaxLength, nameof(email)),
            Normalize(phone, PhoneMaxLength),
            Normalize(message, MessageMaxLength),
            consentGivenOn);
    }

    public void MarkDelivered(DateTimeOffset completedOn)
    {
        DeliveryStatus = ContactRequestDeliveryStatus.Sent;
        DeliveryCompletedOn = completedOn;
        DeliveryAttempts++;
        DeliveryError = null;
    }

    public void MarkDeliveryFailed(DateTimeOffset completedOn, string error)
    {
        DeliveryStatus = ContactRequestDeliveryStatus.Failed;
        DeliveryCompletedOn = completedOn;
        DeliveryAttempts++;
        DeliveryError = Normalize(error, DeliveryErrorMaxLength);
    }

    private static string NormalizeRequired(string? value, int maxLength, string parameterName)
    {
        return Normalize(value, maxLength)
               ?? throw new ArgumentException("Value must not be empty.", parameterName);
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}