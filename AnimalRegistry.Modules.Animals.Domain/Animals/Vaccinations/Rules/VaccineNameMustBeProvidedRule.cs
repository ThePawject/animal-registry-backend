using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations.Rules;

internal sealed class VaccineNameMustBeProvidedRule(string? vaccineName) : IBusinessRule
{
    public string Message => "Vaccine name must be provided.";

    public bool IsBroken()
    {
        return string.IsNullOrWhiteSpace(vaccineName);
    }
}
