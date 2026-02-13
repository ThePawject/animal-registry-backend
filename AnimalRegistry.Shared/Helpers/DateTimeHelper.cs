namespace AnimalRegistry.Shared.Helpers;

public static class DateTimeHelper
{
    public static DateTimeOffset GetQuarterStart(DateTimeOffset referenceDate)
    {
        return referenceDate.AddDays(-90);
    }

    public static DateTimeOffset GetMonthStart(DateTimeOffset referenceDate)
    {
        return referenceDate.AddDays(-30);
    }

    public static DateTimeOffset GetWeekStart(DateTimeOffset referenceDate)
    {
        return referenceDate.AddDays(-7);
    }

    public static PeriodRange GetReportPeriods(DateTimeOffset referenceDate)
    {
        return new PeriodRange(
            GetQuarterStart(referenceDate),
            GetMonthStart(referenceDate),
            GetWeekStart(referenceDate),
            referenceDate
        );
    }
}

public readonly record struct PeriodRange(
    DateTimeOffset QuarterStart,
    DateTimeOffset MonthStart,
    DateTimeOffset WeekStart,
    DateTimeOffset EndDate
);
