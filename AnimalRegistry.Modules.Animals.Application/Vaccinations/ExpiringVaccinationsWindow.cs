using AnimalRegistry.Modules.Animals.Domain;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

public static class ExpiringVaccinationsWindow
{
    public static (DateTimeOffset? ValidFrom, DateTimeOffset ValidBefore) Resolve(int expiresInDays,
        bool includeExpired, DateTimeOffset now)
    {
        var today = ShelterCalendar.ToLocal(now).Date;
        var validBefore = ShelterCalendar.StartOfDay(today.AddDays(expiresInDays + 1));
        return (includeExpired ? null : ShelterCalendar.StartOfDay(today), validBefore);
    }
}
