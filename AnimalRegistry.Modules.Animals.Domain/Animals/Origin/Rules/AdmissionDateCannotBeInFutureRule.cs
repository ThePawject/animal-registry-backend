using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.Origin.Rules;

internal sealed class AdmissionDateCannotBeInFutureRule(DateTimeOffset admissionDate, DateTimeOffset now)
    : IBusinessRule
{
    public string Message => "Admission date cannot be in the future.";

    public bool IsBroken()
    {
        return admissionDate >= ShelterCalendar.StartOfNextDay(now);
    }
}
