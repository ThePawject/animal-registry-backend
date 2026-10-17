using AnimalRegistry.Modules.Animals.Domain;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

public static class OriginReportPeriod
{
    public static (DateTimeOffset From, DateTimeOffset To) Month(int year, int month)
    {
        var start = new DateTime(year, month, 1);
        return (ShelterCalendar.StartOfDay(start), ShelterCalendar.StartOfDay(start.AddMonths(1)));
    }

    public static (DateTimeOffset From, DateTimeOffset To) Year(int year)
    {
        var start = new DateTime(year, 1, 1);
        return (ShelterCalendar.StartOfDay(start), ShelterCalendar.StartOfDay(start.AddYears(1)));
    }
}
