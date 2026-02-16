using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.Common;
using QuestPDF.Fluent;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;

internal sealed class RepositoryDumpReportPdfService : ReportPdfBase, IRepositoryDumpReportPdfService
{
    public byte[] GenerateReport(RepositoryDumpReportData data, DateTimeOffset generatedAt)
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
                        "Zrzut Repozytorium Zwierząt",
                        data.ShelterId,
                        generatedAt);


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

                AddFooter(page, generatedAt, data.ShelterId);
            });
        });
    }
}