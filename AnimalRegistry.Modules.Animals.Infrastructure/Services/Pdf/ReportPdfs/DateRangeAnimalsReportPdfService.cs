using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.Common;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;

internal sealed class DateRangeAnimalsReportPdfService : ReportPdfBase, IDateRangeAnimalsReportPdfService
{
    public byte[] GenerateReport(DateRangeAnimalsReportData data, DateTimeOffset generatedAt)
    {
        return GeneratePdfDocument(container =>
        {
            container.Page(page =>
            {
                AddPageConfiguration(page);

                page.Content().Column(column =>
                {
                    AddReportTitle(
                        column,
                        "Raport Zwierząt z Zakresem Dat",
                        data.ShelterId,
                        generatedAt);

                    column.Item().Text($"Okres: {data.StartDate:dd.MM.yyyy} - {data.EndDate:dd.MM.yyyy}")
                        .FontSize(12)
                        .Bold();
                    column.Item().Text($"Liczba zwierząt z wydarzeniami w tym okresie: {data.TotalAnimals}")
                        .FontSize(12);
                    column.Item().Text($"Łączna liczba wydarzeń: {data.TotalEvents}")
                        .FontSize(12);
                    column.Item().Height(1f, Unit.Centimetre);

                    if (data.Animals.Count == 0)
                    {
                        ReportComponents.AddEmptyState(column, "Brak zwierząt z wydarzeniami w wybranym okresie.");
                    }
                    else
                    {
                        for (var i = 0; i < data.Animals.Count; i++)
                        {
                            AddAnimalWithEvents(column, data.Animals[i], i, data.StartDate, data.EndDate);
                        }
                    }
                });

                AddFooter(page, generatedAt);
            });
        });
    }

    private static void AddAnimalWithEvents(
        ColumnDescriptor column,
        AnimalWithFilteredEvents animalWithEvents,
        int index,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        if (index > 0)
        {
            column.Item().PageBreak();
        }

        var animal = animalWithEvents.Animal;

        column.Item().Text($"{animal.Name}").FontSize(16).Bold();
        column.Item().Height(0.3f, Unit.Centimetre);

        var basicInfo = new Dictionary<string, string>
        {
            { "ID", animal.Id.ToString() },
            { "Sygnatura", animal.Signature.Value },
            { "Gatunek", AnimalPdfComponents.GetSpeciesName(animal.Species) },
            { "Płeć", AnimalPdfComponents.GetSexName(animal.Sex) },
            { "Kolor", animal.Color },
            { "Data urodzenia", animal.BirthDate.ToString("dd.MM.yyyy") },
            { "W schronisku", animal.IsInShelter ? "Tak" : "Nie" },
        };

        ReportComponents.AddInfoTable(column, basicInfo);

        column.Item().Height(0.5f, Unit.Centimetre);
        column.Item().Text($"Zdarzenia w okresie {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}")
            .FontSize(12)
            .Bold();

        if (animalWithEvents.Events.Count == 0)
        {
            ReportComponents.AddEmptyState(column, "Brak zdarzeń w wybranym okresie.");
        }
        else
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                });

                table.Header(header =>
                {
                    header.Cell().Element(ReportStyles.HeaderStyle).Text("Data").Bold();
                    header.Cell().Element(ReportStyles.HeaderStyle).Text("Typ").Bold();
                    header.Cell().Element(ReportStyles.HeaderStyle).Text("Wykonane przez").Bold();
                    header.Cell().Element(ReportStyles.HeaderStyle).Text("Opis").Bold();
                });

                foreach (var evt in animalWithEvents.Events)
                {
                    table.Cell().Element(ReportStyles.CellStyle).Text(evt.OccurredOn.ToString("dd.MM.yyyy"));
                    table.Cell().Element(ReportStyles.CellStyle).Text(AnimalPdfComponents.GetEventTypeName(evt.Type));
                    table.Cell().Element(ReportStyles.CellStyle).Text(evt.PerformedBy);
                    table.Cell().Element(ReportStyles.CellStyle)
                        .Text(string.IsNullOrWhiteSpace(evt.Description) ? "-" : evt.Description);
                }
            });
        }
    }
}