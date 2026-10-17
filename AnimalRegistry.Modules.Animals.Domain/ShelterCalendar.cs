namespace AnimalRegistry.Modules.Animals.Domain;

public static class ShelterCalendar
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

    public static DateTime ToLocal(DateTimeOffset value)
    {
        return TimeZoneInfo.ConvertTime(value, Zone).DateTime;
    }

    public static DateTimeOffset StartOfDay(DateTime localDate)
    {
        var date = DateTime.SpecifyKind(localDate.Date, DateTimeKind.Unspecified);
        return new DateTimeOffset(date, Zone.GetUtcOffset(date));
    }

    public static DateTimeOffset StartOfNextDay(DateTimeOffset now)
    {
        return StartOfDay(ToLocal(now).AddDays(1));
    }
}
