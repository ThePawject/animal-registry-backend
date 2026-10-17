using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;
using AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.PdfHelpers;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.ReportPdfs;

public class OriginReportPdfServiceTests
{
    private static OriginReportEntry CreateEntry()
    {
        var animal = Animal.Create(AnimalSignature.Create("2026/0001").Value!, null, "Burek", "Brown", "Mix", "",
            AnimalSpecies.Dog, AnimalSex.Male, null, "shelter");
        return new OriginReportEntry
        {
            Animal = animal, Origin = AnimalOriginDto.Unassigned, AdmittedOn = animal.CreatedOn,
        };
    }

    [Fact]
    public void MonthlyReport_ShouldCreateValidPdf()
    {
        var data = new OriginMonthlyReportData
        {
            ShelterId = "shelter",
            Year = 2026,
            Month = 3,
            ReportDate = DateTimeOffset.UtcNow,
            Groups =
            [
                new OriginContractorGroup
                {
                    ContractorName = OriginLabels.Unassigned, IsUnassigned = true, Entries = [CreateEntry()],
                },
            ],
        };

        var pdf = new OriginMonthlyReportPdfService().GenerateReport(data, DateTimeOffset.UtcNow);

        PdfTestHelpers.AssertValidPdfStructure(pdf);
    }

    [Fact]
    public void MonthlyReport_ShouldCreateValidPdf_WhenThereAreNoAnimals()
    {
        var data = new OriginMonthlyReportData
        {
            ShelterId = "shelter", Year = 2026, Month = 3, ReportDate = DateTimeOffset.UtcNow, Groups = [],
        };

        var pdf = new OriginMonthlyReportPdfService().GenerateReport(data, DateTimeOffset.UtcNow);

        PdfTestHelpers.AssertValidPdfStructure(pdf);
    }

    [Fact]
    public void YearlyReport_ShouldCreateValidPdf()
    {
        var data = new OriginYearlyReportData
        {
            ShelterId = "shelter",
            Year = 2026,
            ReportDate = DateTimeOffset.UtcNow,
            Rows =
            [
                new OriginYearlyRow
                {
                    ContractorName = "Ząbki", IsUnassigned = false, MonthCounts = [1, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 3],
                },
            ],
        };

        var pdf = new OriginYearlyReportPdfService().GenerateReport(data, DateTimeOffset.UtcNow);

        PdfTestHelpers.AssertValidPdfStructure(pdf);
    }
}
