using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Municipalities.Rules;

internal sealed class MunicipalityNameMustBeProvidedRule(string? name) : IBusinessRule
{
    public string Message => "Municipality name must be provided.";

    public bool IsBroken()
    {
        return string.IsNullOrWhiteSpace(name);
    }
}
