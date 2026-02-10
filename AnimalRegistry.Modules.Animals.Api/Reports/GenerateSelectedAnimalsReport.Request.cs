using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

public sealed class GenerateSelectedAnimalsReportRequest
{
    public const string Route = "/reports/animals/selected";

    [QueryParam]
    public List<Guid> Ids { get; init; } = [];
}
