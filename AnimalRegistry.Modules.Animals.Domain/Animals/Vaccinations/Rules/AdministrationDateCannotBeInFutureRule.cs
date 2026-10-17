using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations.Rules;

internal sealed class AdministrationDateCannotBeInFutureRule(DateTimeOffset administeredOn, DateTimeOffset now)
    : IBusinessRule
{
    public string Message => "Vaccination date cannot be in the future.";

    public bool IsBroken()
    {
        return administeredOn >= ShelterCalendar.StartOfNextDay(now);
    }
}
