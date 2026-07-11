using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateEventReportRequest
{
    [QueryParam] public List<string>? Periods { get; init; }

    [QueryParam] public DateTimeOffset? CustomStartDate { get; init; }

    [QueryParam] public DateTimeOffset? CustomEndDate { get; init; }
}
