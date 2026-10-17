using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;
using AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.PdfHelpers;
using NSubstitute;
using static AnimalRegistry.Modules.Animals.Tests.Unit.VaccinationTestData;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.ReportPdfs;

public class VaccinationReportPdfServiceTests
{
    private static VaccinationWithAnimalDto CreateEntry(DateTimeOffset? validUntil)
    {
        var info = new VaccinationWithAnimalInfo(CreateVaccination(validUntil: validUntil), "Burek", "2026/0001",
            AnimalSpecies.Dog, true);
        return VaccinationWithAnimalDto.FromDomain(info, Now, Substitute.For<IBlobStorageService>());
    }

    [Fact]
    public void ExpiringReport_ShouldCreateValidPdf()
    {
        var data = new ExpiringVaccinationsReportData
        {
            ShelterId = "shelter",
            ExpiresInDays = 30,
            IncludeExpired = true,
            InShelterOnly = true,
            Entries =
            [
                CreateEntry(LocalDate(2026, 3, 12)), CreateEntry(LocalDate(2026, 3, 15)),
                CreateEntry(LocalDate(2026, 4, 1)),
            ],
            ReportDate = Now,
        };

        var pdf = new ExpiringVaccinationsReportPdfService().GenerateReport(data, Now);

        PdfTestHelpers.AssertValidPdfStructure(pdf);
    }

    [Fact]
    public void ExpiringReport_ShouldCreateValidPdf_WhenThereAreNoVaccinations()
    {
        var data = new ExpiringVaccinationsReportData
        {
            ShelterId = "shelter",
            ExpiresInDays = 30,
            IncludeExpired = false,
            InShelterOnly = false,
            Entries = [],
            ReportDate = Now,
        };

        var pdf = new ExpiringVaccinationsReportPdfService().GenerateReport(data, Now);

        PdfTestHelpers.AssertValidPdfStructure(pdf);
    }

    [Fact]
    public void PeriodReport_ShouldCreateValidPdf()
    {
        var data = new VaccinationsReportData
        {
            ShelterId = "shelter",
            From = new DateTime(2026, 3, 1),
            To = new DateTime(2026, 3, 31),
            Entries = [CreateEntry(null), CreateEntry(LocalDate(2027, 3, 10))],
            ReportDate = Now,
        };

        var pdf = new VaccinationsReportPdfService().GenerateReport(data, Now);

        PdfTestHelpers.AssertValidPdfStructure(pdf);
    }

    [Fact]
    public void PeriodReport_ShouldCreateValidPdf_WhenThereAreNoVaccinations()
    {
        var data = new VaccinationsReportData
        {
            ShelterId = "shelter",
            From = new DateTime(2026, 3, 1),
            To = new DateTime(2026, 3, 31),
            Entries = [],
            ReportDate = Now,
        };

        var pdf = new VaccinationsReportPdfService().GenerateReport(data, Now);

        PdfTestHelpers.AssertValidPdfStructure(pdf);
    }
}
