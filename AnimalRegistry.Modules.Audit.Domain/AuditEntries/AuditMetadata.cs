using AnimalRegistry.Modules.Audit.Domain.AuditEntries.Rules;
using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Audit.Domain.AuditEntries;

public class AuditMetadata : ValueObject
{
    private AuditMetadata()
    {
    }

    private AuditMetadata(
        string userId,
        string email,
        string shelterId,
        string? ipAddress,
        string? userAgent)
    {
        UserId = userId;
        Email = email;
        ShelterId = shelterId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public string UserId { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string ShelterId { get; private set; } = default!;
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public static AuditMetadata Create(
        string userId,
        string email,
        string shelterId,
        string? ipAddress = null,
        string? userAgent = null)
    {
        CheckRule(new UserIdMustNotBeEmptyRule(userId));
        CheckRule(new EmailMustNotBeEmptyRule(email));
        CheckRule(new ShelterIdMustNotBeEmptyRule(shelterId));

        return new AuditMetadata(userId, email, shelterId, ipAddress, userAgent);
    }
}