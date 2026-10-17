using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Origin.Rules;

internal sealed class FoundStreetMustBeProvidedRule(string? foundStreet) : IBusinessRule
{
    public string Message => "Street where the animal was found must be provided.";

    public bool IsBroken()
    {
        return string.IsNullOrWhiteSpace(foundStreet);
    }
}
