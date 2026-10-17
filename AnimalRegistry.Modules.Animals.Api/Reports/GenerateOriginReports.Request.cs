using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

public sealed class GenerateOriginMonthlyReportRequest
{
    public const string Route = "/reports/origin/monthly";

    [QueryParam] public int Year { get; init; }

    [QueryParam] public int Month { get; init; }
}

public sealed class GenerateOriginYearlyReportRequest
{
    public const string Route = "/reports/origin/yearly";

    [QueryParam] public int Year { get; init; }
}
