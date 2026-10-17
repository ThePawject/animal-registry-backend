namespace AnimalRegistry.Modules.Animals.Application.Reports;

public static class OriginReportPeriod
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

    public static (DateTimeOffset From, DateTimeOffset To) Month(int year, int month)
    {
        var start = new DateTime(year, month, 1);
        return (ToZoned(start), ToZoned(start.AddMonths(1)));
    }

    public static (DateTimeOffset From, DateTimeOffset To) Year(int year)
    {
        var start = new DateTime(year, 1, 1);
        return (ToZoned(start), ToZoned(start.AddYears(1)));
    }

    public static DateTimeOffset StartOfNextDay(DateTimeOffset value)
    {
        return ToZoned(ToLocal(value).Date.AddDays(1));
    }

    public static DateTime ToLocal(DateTimeOffset value)
    {
        return TimeZoneInfo.ConvertTime(value, Zone).DateTime;
    }

    private static DateTimeOffset ToZoned(DateTime local)
    {
        return new DateTimeOffset(local, Zone.GetUtcOffset(local));
    }
}
