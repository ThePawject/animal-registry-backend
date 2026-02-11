using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;

namespace AnimalRegistry.Modules.Animals.Application.Reports.Models;

public sealed class RepositoryDumpReportData
{
    public required string ShelterId { get; init; }
    public required IReadOnlyList<Animal> Animals { get; init; }
    public required DateTimeOffset ReportDate { get; init; }
    public int TotalAnimals => Animals.Count;
}

public sealed class SelectedAnimalsReportData
{
    public required string ShelterId { get; init; }
    public required IReadOnlyList<Animal> Animals { get; init; }
    public required DateTimeOffset ReportDate { get; init; }
    public int TotalAnimals => Animals.Count;
    public IReadOnlyList<Guid> RequestedIds { get; init; } = [];
    public int FoundAnimals => Animals.Count;
    public int MissingAnimals => RequestedIds.Count - FoundAnimals;
}

public sealed class DateRangeAnimalsReportData
{
    public required string ShelterId { get; init; }
    public required DateTimeOffset StartDate { get; init; }
    public required DateTimeOffset EndDate { get; init; }
    public required IReadOnlyList<AnimalWithFilteredEvents> Animals { get; init; }
    public required DateTimeOffset ReportDate { get; init; }
    public int TotalAnimals => Animals.Count;
    public int TotalEvents => Animals.Sum(a => a.Events.Count);
}

public sealed class AnimalWithFilteredEvents
{
    public required Animal Animal { get; init; }
    public required IReadOnlyList<AnimalEvent> Events { get; init; }
}
