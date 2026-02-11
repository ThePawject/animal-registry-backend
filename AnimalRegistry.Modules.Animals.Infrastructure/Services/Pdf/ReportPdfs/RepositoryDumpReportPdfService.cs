using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;

internal sealed class RepositoryDumpReportPdfService : ReportPdfBase, IRepositoryDumpReportPdfService
{
    public byte[] GenerateReport(RepositoryDumpReportData data, DateTimeOffset generatedAt)
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
                        "Zrzut Repozytorium Zwierząt",
                        data.ShelterId,
                        generatedAt);
                    
                    column.Item().Text($"Raport zawiera pełny zrzut wszystkich danych o zwierzętach w systemie.")
                        .FontSize(12);
                    column.Item().Text($"Łączna liczba zwierząt: {data.TotalAnimals}")
                        .FontSize(12)
                        .Bold();
                    column.Item().Height(1f, Unit.Centimetre);
                    
                    if (data.Animals.Count == 0)
                    {
                        ReportComponents.AddEmptyState(column, "Brak zwierząt w bazie danych.");
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
