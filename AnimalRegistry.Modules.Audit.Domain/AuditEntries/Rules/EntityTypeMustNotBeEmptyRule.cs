using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Audit.Domain.AuditEntries.Rules;

internal sealed class EntityTypeMustNotBeEmptyRule(string entityType) : IBusinessRule
{
    public string Message => "Entity type cannot be null or whitespace";

    public bool IsBroken()
    {
        return string.IsNullOrWhiteSpace(entityType);
    }
}