using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services;

internal sealed class EventReportPdfService : IEventReportPdfService
{
    public EventReportPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }
    
    public byte[] GenerateReport(EventReportData data, DateTimeOffset generatedAt)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2f, Unit.Centimetre);
                page.PageColor(Colors.White);
                
                page.Content().Column(column =>
                {
                    AddFirstPageContent(column, data, generatedAt);
                    
                    foreach (var speciesStats in data.SpeciesStats)
                    {
                        AddSpeciesSection(column, speciesStats);
                    }
                });
                
                page.Footer().AlignCenter().Text($"Raport wygenerowany: {generatedAt:dd.MM.yyyy HH:mm} | Animal Registry System").FontSize(9);
            });
        });
        
        return document.GeneratePdf();
    }
    
    private static void AddFirstPageContent(ColumnDescriptor column, EventReportData data, DateTimeOffset generatedAt)
    {
        column.Item().AlignCenter().Text("Raport Zdarzeń Zwierząt").FontSize(24).Bold();
        column.Item().AlignCenter().Text($"Schronisko: {data.ShelterId}").FontSize(14);
        column.Item().AlignCenter().Text($"Data wygenerowania: {generatedAt:dd.MM.yyyy HH:mm}").FontSize(14);
        column.Item().Height(1f, Unit.Centimetre);
        column.Item().Text("Raport zawiera zestawienie zdarzeń dla psów i kotów w podziale na okresy: ostatni kwartał, ostatni miesiąc oraz ostatni tydzień.");
        column.Item().Height(1f, Unit.Centimetre);
    }
    
    private static void AddSpeciesSection(ColumnDescriptor column, SpeciesEventStats stats)
    {
        var speciesName = stats.Species == AnimalSpecies.Dog ? "PSY" : "KOTY";
        column.Item().PageBreak();
        column.Item().Text(speciesName).FontSize(18).Bold();
        
        AddPeriodTable(column, "Okres kwartalny", stats.QuarterStats);
        AddPeriodTable(column, "Okres miesięczny", stats.MonthStats);
        AddPeriodTable(column, "Okres tygodniowy", stats.WeekStats);
    }
    
    private static void AddPeriodTable(ColumnDescriptor column, string periodTitle, PeriodStats stats)
    {
        column.Item().Height(0.5f, Unit.Centimetre);
        column.Item().Text(periodTitle).FontSize(14).Bold();
        column.Item().Text($"{stats.PeriodFrom:dd.MM.yyyy} – {stats.PeriodTo:dd.MM.yyyy}").FontSize(11);
        column.Item().Height(0.3f, Unit.Centimetre);
        
        if (stats.EventCounts.Count == 0)
        {
            column.Item().Text("Brak zdarzeń w tym okresie.");
            return;
        }
        
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(1);
            });
            
            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Typ zdarzenia").Bold();
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("Liczba").Bold();
            });
            
            foreach (var eventCount in stats.EventCounts)
            {
                table.Cell().Padding(5).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Text(GetEventTypeName(eventCount.EventType));
                table.Cell().Padding(5).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).AlignCenter().Text(eventCount.Count.ToString());
            }
        });
    }
    
    private static string GetEventTypeName(AnimalEventType eventType)
    {
        return eventType switch
        {
            AnimalEventType.None => "Brak",
            AnimalEventType.AdmissionToShelter => "Przyjęcie do schroniska",
            AnimalEventType.StartOfQuarantine => "Rozpoczęcie kwarantanny",
            AnimalEventType.EndOfQuarantine => "Zakończenie kwarantanny",
            AnimalEventType.InfectiousDiseaseVaccination => "Szczepienie przeciw chorobom zakaźnym",
            AnimalEventType.Deworming => "Odrobaczenie",
            AnimalEventType.Defleaing => "Odpluskwienie",
            AnimalEventType.Sterilization => "Sterylizacja/Kastracja",
            AnimalEventType.RabiesVaccination => "Szczepienie przeciw wściekliźnie",
            AnimalEventType.Adoption => "Adopcja",
            AnimalEventType.Walk => "Spacer",
            AnimalEventType.NewKennelNumber => "Nowy numer kojca",
            AnimalEventType.PickedUpByOwner => "Odbiór przez właściciela",
            AnimalEventType.Weighing => "Ważenie",
            AnimalEventType.Euthanasia => "Eutanazja",
            AnimalEventType.Death => "Śmierć",
            _ => eventType.ToString()
        };
    }
}