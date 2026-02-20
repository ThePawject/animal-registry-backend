using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Audit.Domain.AuditEntries.Rules;

internal sealed class EmailMustNotBeEmptyRule(string email) : IBusinessRule
{
    public string Message => "Email cannot be null or whitespace";

    public bool IsBroken()
    {
        return string.IsNullOrWhiteSpace(email);
    }
}