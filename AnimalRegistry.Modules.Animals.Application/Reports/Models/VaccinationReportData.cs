using AnimalRegistry.Modules.Animals.Application.Vaccinations;

namespace AnimalRegistry.Modules.Animals.Application.Reports.Models;

public sealed class ExpiringVaccinationsReportData
{
    public required string ShelterId { get; init; }
    public required int ExpiresInDays { get; init; }
    public required bool IncludeExpired { get; init; }
    public required bool InShelterOnly { get; init; }
    public required IReadOnlyList<VaccinationWithAnimalDto> Entries { get; init; }
    public required DateTimeOffset ReportDate { get; init; }
}

public sealed class VaccinationsReportData
{
    public required string ShelterId { get; init; }
    public required DateTime From { get; init; }
    public required DateTime To { get; init; }
    public required IReadOnlyList<VaccinationWithAnimalDto> Entries { get; init; }
    public required DateTimeOffset ReportDate { get; init; }
}
