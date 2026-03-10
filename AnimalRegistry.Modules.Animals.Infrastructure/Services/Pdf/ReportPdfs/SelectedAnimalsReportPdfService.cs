using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.Common;
using QuestPDF.Fluent;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;

internal sealed class SelectedAnimalsReportPdfService : ReportPdfBase, ISelectedAnimalsReportPdfService
{
    public byte[] GenerateReport(SelectedAnimalsReportData data, DateTimeOffset generatedAt)
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
                    AddReportTitle(
                        column,
                        "Raport Wybranych Zwierząt",
                        data.ShelterId,
                        generatedAt);


                    if (data.Animals.Count == 0)
                    {
                        ReportComponents.AddEmptyState(column, "Nie znaleziono żadnych zwierząt o podanych ID.");
                    }
                    else
                    {
                        for (var i = 0; i < data.Animals.Count; i++)
                        {
                            AnimalPdfComponents.AddAnimalSection(column, data.Animals[i], i, data.TotalAnimals, true,
                                data.PhotoData);
                        }
                    }
                });

                AddFooter(page, generatedAt, data.ShelterId);
            });
        });
    }
}