using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Audit.Domain.AuditEntries.Rules;

internal sealed class UserIdMustNotBeEmptyRule(string userId) : IBusinessRule
{
    public string Message => "User ID cannot be null or whitespace";

    public bool IsBroken()
    {
        return string.IsNullOrWhiteSpace(userId);
    }
}