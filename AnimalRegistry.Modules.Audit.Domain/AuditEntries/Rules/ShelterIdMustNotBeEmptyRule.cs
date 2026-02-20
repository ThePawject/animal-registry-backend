using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Audit.Domain.AuditEntries.Rules;

internal sealed class ShelterIdMustNotBeEmptyRule(string shelterId) : IBusinessRule
{
    public string Message => "Shelter ID cannot be null or whitespace";

    public bool IsBroken()
    {
        return string.IsNullOrWhiteSpace(shelterId);
    }
}