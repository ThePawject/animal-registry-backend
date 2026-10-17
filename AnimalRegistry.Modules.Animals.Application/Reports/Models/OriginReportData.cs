using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Application.Reports.Models;

public sealed class OriginReportEntry
{
    public required Animal Animal { get; init; }
    public required AnimalOriginDto Origin { get; init; }

    public required DateTimeOffset AdmittedOn { get; init; }
}

public sealed class OriginContractorGroup
{
    public required string ContractorName { get; init; }
    public required bool IsUnassigned { get; init; }
    public required IReadOnlyList<OriginReportEntry> Entries { get; init; }
}

public sealed class OriginMonthlyReportData
{
    public required string ShelterId { get; init; }
    public required int Year { get; init; }
    public required int Month { get; init; }
    public required IReadOnlyList<OriginContractorGroup> Groups { get; init; }
    public required DateTimeOffset ReportDate { get; init; }
    public int TotalAnimals => Groups.Sum(g => g.Entries.Count);
}

public sealed class OriginYearlyRow
{
    public required string ContractorName { get; init; }
    public required bool IsUnassigned { get; init; }

    public required IReadOnlyList<int> MonthCounts { get; init; }

    public int Total => MonthCounts.Sum();
}

public sealed class OriginYearlyReportData
{
    public required string ShelterId { get; init; }
    public required int Year { get; init; }
    public required IReadOnlyList<OriginYearlyRow> Rows { get; init; }
    public required DateTimeOffset ReportDate { get; init; }

    public IReadOnlyList<int> MonthTotals =>
        Enumerable.Range(0, 12).Select(month => Rows.Sum(r => r.MonthCounts[month])).ToList();

    public int Total => Rows.Sum(r => r.Total);
}
