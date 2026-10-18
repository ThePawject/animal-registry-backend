using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations.Rules;

internal sealed class VaccinationCanBeReplacedOnlyOnceRule(bool isReplacedElsewhere) : IBusinessRule
{
    public string Message => "This vaccination has already been replaced by another one.";

    public bool IsBroken()
    {
        return isReplacedElsewhere;
    }
}
