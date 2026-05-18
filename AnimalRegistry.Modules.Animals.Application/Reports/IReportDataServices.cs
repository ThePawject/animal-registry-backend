using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

public interface IRepositoryDumpDataService
{
    Task<RepositoryDumpReportData> PrepareReportDataAsync(
        string shelterId,
        CancellationToken cancellationToken = default);
}

public interface ISelectedAnimalsDataService
{
    Task<SelectedAnimalsReportData> PrepareReportDataAsync(
        string shelterId,
        IReadOnlyList<Guid> animalIds,
        CancellationToken cancellationToken = default);
}

public interface IDateRangeAnimalsDataService
{
    Task<DateRangeAnimalsReportData> PrepareReportDataAsync(
        string shelterId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        IReadOnlyList<AnimalSpecies>? species = null,
        IReadOnlyList<AnimalEventType>? eventTypes = null,
        CancellationToken cancellationToken = default);
}