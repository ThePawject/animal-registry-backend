using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

public sealed class GenerateVaccinationsReportRequest
{
    public const string Route = "/reports/vaccinations";

    [QueryParam] public DateTimeOffset? From { get; init; }

    [QueryParam] public DateTimeOffset? To { get; init; }
}
