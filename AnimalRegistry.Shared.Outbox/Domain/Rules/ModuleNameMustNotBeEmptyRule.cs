using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Shared.Outbox.Domain.Rules;

internal sealed class ModuleNameMustNotBeEmptyRule(string moduleName) : IBusinessRule
{
    public string Message => "Module name cannot be null or whitespace";

    public bool IsBroken()
    {
        return string.IsNullOrWhiteSpace(moduleName);
    }
}