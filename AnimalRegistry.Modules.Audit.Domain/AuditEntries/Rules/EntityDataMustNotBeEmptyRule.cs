using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Audit.Domain.AuditEntries.Rules;

internal sealed class EntityDataMustNotBeEmptyRule(string entityData) : IBusinessRule
{
    public string Message => "Entity data cannot be null or whitespace";

    public bool IsBroken()
    {
        return string.IsNullOrWhiteSpace(entityData);
    }
}