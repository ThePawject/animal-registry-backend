using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations.Rules;

internal sealed class ReplacedVaccinationMustBelongToSameAnimalRule(
    AnimalVaccination replaced,
    Guid animalId,
    string shelterId) : IBusinessRule
{
    public string Message => "Replaced vaccination must belong to the same animal.";

    public bool IsBroken()
    {
        return replaced.AnimalId != animalId || replaced.ShelterId != shelterId;
    }
}
