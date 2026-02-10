using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services;

internal sealed class DateRangeAnimalsReportPdfService : IDateRangeAnimalsReportPdfService
{
    public DateRangeAnimalsReportPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateReport(DateTimeOffset startDate, DateTimeOffset endDate, List<AnimalSpecies>? species, DateTimeOffset generatedAt, string shelterId)
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
                    column.Item().AlignCenter().Text("Raport Zwierzat z Zakresem Dat").FontSize(24).Bold();
                    column.Item().AlignCenter().Text($"Schronisko: {shelterId}").FontSize(14);
                    column.Item().AlignCenter().Text($"Data wygenerowania: {generatedAt:dd.MM.yyyy HH:mm}").FontSize(14);
                    column.Item().Height(2f, Unit.Centimetre);
                    column.Item().Text($"Okres: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}").FontSize(12);
                    column.Item().Height(3f, Unit.Centimetre);
                    column.Item().AlignCenter().Text("TODO").FontSize(48).Bold().FontColor(Colors.Red.Medium);
                });

                page.Footer().AlignCenter().Text($"Raport wygenerowany: {generatedAt:dd.MM.yyyy HH:mm} | Animal Registry System").FontSize(9);
            });
        });

        return document.GeneratePdf();
    }
}
