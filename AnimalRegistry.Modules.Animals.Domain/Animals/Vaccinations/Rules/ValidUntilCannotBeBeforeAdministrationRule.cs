using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations.Rules;

internal sealed class ValidUntilCannotBeBeforeAdministrationRule(
    DateTimeOffset administeredOn,
    DateTimeOffset? validUntil) : IBusinessRule
{
    public string Message => "Vaccination validity date cannot be earlier than the vaccination date.";

    public bool IsBroken()
    {
        return validUntil is { } date &&
               ShelterCalendar.ToLocal(date).Date < ShelterCalendar.ToLocal(administeredOn).Date;
    }
}
