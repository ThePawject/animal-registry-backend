using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
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
        
        var quarterStart = generatedAt.AddDays(-90);
        var monthStart = generatedAt.AddDays(-30);
        var weekStart = generatedAt.AddDays(-7);
        
        var dogEvents = events.Where(e => e.Species == AnimalSpecies.Dog).Select(e => e.AnimalEvent).ToList();
        var catEvents = events.Where(e => e.Species == AnimalSpecies.Cat).Select(e => e.AnimalEvent).ToList();
        
        var reportData = new EventReportData
        {
            ShelterId = currentUser.ShelterId,
            ReportDate = generatedAt,
            SpeciesStats =
            [
                CreateSpeciesStats(AnimalSpecies.Dog, dogEvents, quarterStart, monthStart, weekStart, generatedAt),
                CreateSpeciesStats(AnimalSpecies.Cat, catEvents, quarterStart, monthStart, weekStart, generatedAt)
            ]
        };
        
        var pdfBytes = pdfService.GenerateReport(reportData, generatedAt);
        var fileName = $"RaportZdarzen_{generatedAt:dd_MM_yyyy}.pdf";
        
        return Result<GenerateEventReportResponse>.Success(new GenerateEventReportResponse
        {
            FileName = fileName,
            ContentType = "application/pdf",
            Data = pdfBytes
        });
    }
    
    private static SpeciesEventStats CreateSpeciesStats(
        AnimalSpecies species,
        List<AnimalEvent> events,
        DateTimeOffset quarterStart,
        DateTimeOffset monthStart,
        DateTimeOffset weekStart,
        DateTimeOffset endDate)
    {
        return new SpeciesEventStats
        {
            Species = species,
            QuarterStats = CreatePeriodStats(events, quarterStart, endDate),
            MonthStats = CreatePeriodStats(events, monthStart, endDate),
            WeekStats = CreatePeriodStats(events, weekStart, endDate)
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
        
        return new PeriodStats
        {
            PeriodFrom = from,
            PeriodTo = to,
            EventCounts = eventCounts
        };
    }
}