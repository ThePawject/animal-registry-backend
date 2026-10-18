using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using System.Globalization;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.ReportData;

internal sealed class OriginReportDataService(
    IOriginReportQuery query,
    IMunicipalityRepository municipalityRepository) : IOriginReportDataService
{
    private static readonly StringComparer PolishComparer = StringComparer.Create(new CultureInfo("pl-PL"), true);

    public async Task<OriginMonthlyReportData> PrepareMonthlyReportDataAsync(
        string shelterId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var (from, to) = OriginReportPeriod.Month(year, month);
        var entries = await LoadEntriesAsync(shelterId, from, to, cancellationToken);

        var groups = GroupByContractor(entries)
            .Select(g => new OriginContractorGroup
            {
                ContractorName = g.Name,
                IsUnassigned = g.IsUnassigned,
                Entries = g.Entries
                    .OrderBy(e => e.AdmittedOn)
                    .ThenBy(e => e.Animal.Signature.Value)
                    .ToList(),
            })
            .ToList();

        return new OriginMonthlyReportData
        {
            ShelterId = shelterId,
            Year = year,
            Month = month,
            Groups = groups,
            ReportDate = DateTimeOffset.UtcNow,
        };
    }

    public async Task<OriginYearlyReportData> PrepareYearlyReportDataAsync(
        string shelterId,
        int year,
        CancellationToken cancellationToken = default)
    {
        var (from, to) = OriginReportPeriod.Year(year);
        var entries = await LoadEntriesAsync(shelterId, from, to, cancellationToken);

        var rows = GroupByContractor(entries)
            .Select(g =>
            {
                var counts = new int[12];
                foreach (var entry in g.Entries)
                {
                    counts[ShelterCalendar.ToLocal(entry.AdmittedOn).Month - 1]++;
                }

                return new OriginYearlyRow
                {
                    ContractorName = g.Name, IsUnassigned = g.IsUnassigned, MonthCounts = counts,
                };
            })
            .ToList();

        return new OriginYearlyReportData
        {
            ShelterId = shelterId, Year = year, Rows = rows, ReportDate = DateTimeOffset.UtcNow,
        };
    }

    private async Task<IReadOnlyList<OriginReportEntry>> LoadEntriesAsync(
        string shelterId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var admitted = await query.GetAdmittedBetweenAsync(shelterId, from, to, cancellationToken);
        var municipalities = await municipalityRepository.GetByIdsAsync(
            admitted
                .Select(a => a.Animal.Origin)
                .OfType<AnimalOrigin>()
                .SelectMany(o => new[] { o.FoundMunicipalityId, o.ContractorMunicipalityId }),
            shelterId,
            cancellationToken);
        var lookup = new AnimalOriginLookup(municipalities.ToDictionary(m => m.Id, m => m.Name));

        return admitted
            .Select(a => new OriginReportEntry
            {
                Animal = a.Animal, Origin = lookup.Resolve(a.Animal), AdmittedOn = a.AdmittedOn,
            })
            .ToList();
    }

    private static IEnumerable<(string Name, bool IsUnassigned, IReadOnlyList<OriginReportEntry> Entries)>
        GroupByContractor(IEnumerable<OriginReportEntry> entries)
    {
        return entries
            .GroupBy(e => e.Origin.ContractorMunicipality)
            .Select(g => (
                g.Key.Name,
                IsUnassigned: g.Key.Id is null,
                Entries: (IReadOnlyList<OriginReportEntry>)g.ToList()))
            .OrderBy(g => g.IsUnassigned)
            .ThenBy(g => g.Name, PolishComparer);
    }
}
