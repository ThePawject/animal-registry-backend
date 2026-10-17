using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

public sealed class GenerateExpiringVaccinationsReportRequest
{
    public const string Route = "/reports/vaccinations/expiring";

    [QueryParam] public int ExpiresInDays { get; init; } = 30;

    [QueryParam] public bool IncludeExpired { get; init; }

    [QueryParam] public bool InShelterOnly { get; init; } = true;
}
