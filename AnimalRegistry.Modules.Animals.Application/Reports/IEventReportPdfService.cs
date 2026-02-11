using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

public interface IEventReportPdfService
{
    byte[] GenerateReport(EventReportData data, DateTimeOffset generatedAt);
}

public sealed class EventReportData
{
    public required string ShelterId { get; init; }
    public required List<SpeciesEventStats> SpeciesStats { get; init; }
    public required DateTimeOffset ReportDate { get; init; }
}

public sealed class SpeciesEventStats
{
    public required AnimalSpecies Species { get; init; }
    public required PeriodStats QuarterStats { get; init; }
    public required PeriodStats MonthStats { get; init; }
    public required PeriodStats WeekStats { get; init; }
}

public sealed class PeriodStats
{
    public required DateTimeOffset PeriodFrom { get; init; }
    public required DateTimeOffset PeriodTo { get; init; }
    public required List<EventTypeCount> EventCounts { get; init; }
}

public sealed class EventTypeCount
{
    public required AnimalEventType EventType { get; init; }
    public required int Count { get; init; }
}