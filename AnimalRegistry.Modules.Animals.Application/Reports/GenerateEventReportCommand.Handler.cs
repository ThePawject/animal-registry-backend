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

        var periods = DateTimeHelper.GetReportPeriods(generatedAt);

        var dogEvents = events.Where(e => e.Species == AnimalSpecies.Dog).Select(e => e.AnimalEvent).ToList();
        var catEvents = events.Where(e => e.Species == AnimalSpecies.Cat).Select(e => e.AnimalEvent).ToList();

        var reportData = new EventReportData
        {
            ShelterId = currentUser.ShelterId,
            ReportDate = generatedAt,
            SpeciesStats =
            [
                CreateSpeciesStats(AnimalSpecies.Dog, dogEvents, periods),
                CreateSpeciesStats(AnimalSpecies.Cat, catEvents, periods),
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
        PeriodRange periods)
    {
        return new SpeciesEventStats
        {
            Species = species,
            QuarterStats = CreatePeriodStats(events, periods.QuarterStart, periods.EndDate),
            MonthStats = CreatePeriodStats(events, periods.MonthStart, periods.EndDate),
            WeekStats = CreatePeriodStats(events, periods.WeekStart, periods.EndDate),
        };
    }

    private static PeriodStats CreatePeriodStats(List<AnimalEvent> events, DateTimeOffset from, DateTimeOffset to)
    {
        var periodEvents = events.Where(e => e.OccurredOn >= from && e.OccurredOn <= to).ToList();

        var eventCounts = periodEvents
            .GroupBy(e => e.Type)
            .Select(g => new EventTypeCount { EventType = g.Key, Count = g.Count() })
            .OrderBy(ec => ec.EventType)
            .ToList();

        return new PeriodStats { PeriodFrom = from, PeriodTo = to, EventCounts = eventCounts };
    }
}