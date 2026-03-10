using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.Common;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;

internal sealed class EventReportPdfService : ReportPdfBase, IEventReportPdfService
{
    public byte[] GenerateReport(EventReportData data, DateTimeOffset generatedAt)
    {
        return GeneratePdfDocument(container =>
        {
            container.Page(page =>
            {
                AddCoverPage(page, data.ShelterId);
            });

            container.Page(page =>
            {
                AddPageConfiguration(page);

                page.Content().Column(column =>
                {
                    var speciesList = data.SpeciesStats.ToList();
                    for (var i = 0; i < speciesList.Count; i++)
                    {
                        AddSpeciesSection(column, speciesList[i], i == 0);
                    }
                });

                page.Footer().AlignCenter()
                    .Text($"Raport wygenerowany: {generatedAt:dd.MM.yyyy HH:mm} | Raport-Zdarzen | {data.ShelterId}")
                    .FontSize(9);
            });
        });
    }

    private static void AddSpeciesSection(ColumnDescriptor column, SpeciesEventStats stats, bool isFirst)
    {
        var speciesName = stats.Species == AnimalSpecies.Dog ? "PSY" : "KOTY";

        if (isFirst)
        {
            column.Item().Text(speciesName).FontSize(18).Bold();
        }
        else
        {
            AddSectionTitle(column, speciesName);
        }

        AddPeriodTable(column, "Okres kwartalny", stats.QuarterStats);
        AddPeriodTable(column, "Okres miesięczny", stats.MonthStats);
        AddPeriodTable(column, "Okres tygodniowy", stats.WeekStats);
    }

    private static void AddPeriodTable(ColumnDescriptor column, string periodTitle, PeriodStats stats)
    {
        AddSubsectionTitle(column, periodTitle);
        column.Item().Text($"{stats.PeriodFrom:dd.MM.yyyy} – {stats.PeriodTo:dd.MM.yyyy}").FontSize(11);
        column.Item().Height(0.3f, Unit.Centimetre);

        if (stats.EventCounts.Count == 0)
        {
            ReportComponents.AddEmptyState(column, "Brak zdarzeń w tym okresie.");
            return;
        }

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Element(ReportStyles.HeaderStyle).Text("Typ zdarzenia").Bold();
                header.Cell().Element(ReportStyles.HeaderStyle).AlignCenter().Text("Liczba").Bold();
            });

            foreach (var eventCount in stats.EventCounts)
            {
                table.Cell().Element(ReportStyles.CellStyle)
                    .Text(AnimalPdfComponents.GetEventTypeName(eventCount.EventType));
                table.Cell().Element(ReportStyles.CellStyle).AlignCenter().Text(eventCount.Count.ToString());
            }
        });
    }
}