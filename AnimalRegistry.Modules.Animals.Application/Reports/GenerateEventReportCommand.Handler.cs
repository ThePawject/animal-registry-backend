using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.Helpers;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

internal sealed class GenerateEventReportCommandHandler(
    IAnimalEventRepository animalEventRepository,
    ICurrentUser currentUser,
    IEventReportPdfService pdfService)
    : IRequestHandler<GenerateEventReportCommand, Result<GenerateEventReportResponse>>
{
    public async Task<Result<GenerateEventReportResponse>> Handle(GenerateEventReportCommand request,
        CancellationToken cancellationToken)
    {
        var generatedAt = DateTimeOffset.UtcNow;

        var events = await animalEventRepository.GetAllByShelterIdAsync(currentUser.ShelterId, cancellationToken);

        var selectedPeriods = GetSelectedPeriods(request.Periods);

        var dogEvents = events.Where(e => e.Species == AnimalSpecies.Dog).Select(e => e.AnimalEvent).ToList();
        var catEvents = events.Where(e => e.Species == AnimalSpecies.Cat).Select(e => e.AnimalEvent).ToList();

        var reportData = new EventReportData
        {
            ShelterId = currentUser.ShelterId,
            ReportDate = generatedAt,
            SpeciesStats =
            [
                CreateSpeciesStats(AnimalSpecies.Dog, dogEvents, selectedPeriods, generatedAt, request),
                CreateSpeciesStats(AnimalSpecies.Cat, catEvents, selectedPeriods, generatedAt, request),
            ],
        };

        var pdfBytes = pdfService.GenerateReport(reportData, generatedAt);
        var fileName = $"RaportZdarzen_{generatedAt:dd_MM_yyyy}.pdf";

        return Result<GenerateEventReportResponse>.Success(new GenerateEventReportResponse
        {
            FileName = fileName, ContentType = "application/pdf", Data = pdfBytes,
        });
    }

    private static SpeciesEventStats CreateSpeciesStats(
        AnimalSpecies species,
        List<AnimalEvent> events,
        IReadOnlyCollection<EventReportPeriod> selectedPeriods,
        DateTimeOffset generatedAt,
        GenerateEventReportCommand request)
    {
        return new SpeciesEventStats
        {
            Species = species,
            PeriodStats = selectedPeriods.Select(period => CreatePeriodStats(events, period, generatedAt, request))
                .ToList(),
        };
    }

    private static IReadOnlyCollection<EventReportPeriod> GetSelectedPeriods(List<EventReportPeriod>? periods)
    {
        return periods is { Count: > 0 }
            ? periods.Distinct().ToList()
            : [EventReportPeriod.Quarter, EventReportPeriod.Month, EventReportPeriod.Week];
    }

    private static PeriodStats CreatePeriodStats(
        List<AnimalEvent> events,
        EventReportPeriod period,
        DateTimeOffset generatedAt,
        GenerateEventReportCommand request)
    {
        var reportPeriods = DateTimeHelper.GetReportPeriods(generatedAt);
        var (title, from, to) = period switch
        {
            EventReportPeriod.Week => ("Okres tygodniowy", reportPeriods.WeekStart, reportPeriods.EndDate),
            EventReportPeriod.Month => ("Okres miesięczny", reportPeriods.MonthStart, reportPeriods.EndDate),
            EventReportPeriod.Quarter => ("Okres kwartalny", reportPeriods.QuarterStart, reportPeriods.EndDate),
            EventReportPeriod.Custom => ("Własny zakres", request.CustomStartDate!.Value, request.CustomEndDate!.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(period), period, null),
        };

        var periodEvents = events.Where(e => e.OccurredOn >= from && e.OccurredOn <= to).ToList();

        var eventCounts = periodEvents
            .GroupBy(e => e.Type)
            .Select(g => new EventTypeCount { EventType = g.Key, Count = g.Count() })
            .OrderBy(ec => ec.EventType)
            .ToList();

        return new PeriodStats { Title = title, PeriodFrom = from, PeriodTo = to, EventCounts = eventCounts };
    }
}
