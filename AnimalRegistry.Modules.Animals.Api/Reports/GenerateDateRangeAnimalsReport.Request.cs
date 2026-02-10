using FastEndpoints;
using System.Collections.Generic;
using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

public sealed class GenerateDateRangeAnimalsReportRequest
{
    public const string Route = "/reports/animals/date-range";

    [QueryParam]
    public DateTimeOffset StartDate { get; init; }

    [QueryParam]
    public DateTimeOffset EndDate { get; init; }

    [QueryParam]
    public List<AnimalSpecies>? Species { get; init; }
}
