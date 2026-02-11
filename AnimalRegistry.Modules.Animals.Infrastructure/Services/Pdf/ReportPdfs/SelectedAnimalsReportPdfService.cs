using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;

internal sealed class SelectedAnimalsReportPdfService : ReportPdfBase, ISelectedAnimalsReportPdfService
{
    public byte[] GenerateReport(SelectedAnimalsReportData data, DateTimeOffset generatedAt)
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
                        "Raport Wybranych Zwierząt",
                        data.ShelterId,
                        generatedAt);
                    
                    column.Item().Text($"Liczba wybranych zwierząt: {data.RequestedIds.Count}")
                        .FontSize(12);
                    column.Item().Text($"Znaleziono: {data.FoundAnimals}")
                        .FontSize(12)
                        .Bold();
                    
                    if (data.MissingAnimals > 0)
                    {
                        column.Item().Text($"Brak w bazie: {data.MissingAnimals}")
                            .FontSize(12)
                            .FontColor(Colors.Red.Medium)
                            .Bold();
                    }
                    
                    column.Item().Height(1f, Unit.Centimetre);
                    
                    if (data.Animals.Count == 0)
                    {
                        ReportComponents.AddEmptyState(column, "Nie znaleziono żadnych zwierząt o podanych ID.");
                    }
                    else
                    {
                        for (var i = 0; i < data.Animals.Count; i++)
                        {
                            AnimalPdfComponents.AddAnimalSection(column, data.Animals[i], i, data.TotalAnimals);
                        }
                    }
                });
                
                AddFooter(page, generatedAt);
            });
        });
    }
}
