using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations.Rules;

internal sealed class ReplacedVaccinationMustBeEarlierRule(AnimalVaccination replaced, DateTimeOffset administeredOn)
    : IBusinessRule
{
    public string Message => "Replaced vaccination must have been administered before the replacing one.";

    public bool IsBroken()
    {
        return replaced.AdministeredOn >= administeredOn;
    }
}
