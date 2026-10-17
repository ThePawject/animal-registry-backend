using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Origin.Rules;

internal sealed class MunicipalityMustBeProvidedRule(Guid municipalityId, string role) : IBusinessRule
{
    public string Message => $"The {role} municipality must be provided.";

    public bool IsBroken()
    {
        return municipalityId == Guid.Empty;
    }
}
