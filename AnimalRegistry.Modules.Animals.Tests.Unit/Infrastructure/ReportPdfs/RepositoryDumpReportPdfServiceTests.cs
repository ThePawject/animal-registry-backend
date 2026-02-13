using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;
using AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.PdfHelpers;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.ReportPdfs;

public class RepositoryDumpReportPdfServiceTests
{
    private readonly RepositoryDumpReportPdfService _pdfService = new();

    [Fact]
    public void GenerateReport_WithValidData_ShouldCreateValidPdfStructure()
    {
        var reportData = CreateValidReportData();
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
    }

    [Fact]
    public void GenerateReport_WithAnimal_ShouldCreateLargerPdf()
    {
        var animal = CreateTestAnimal("Burek", AnimalSpecies.Dog, AnimalSex.Male);
        var reportData = CreateReportDataWithAnimals(new List<Animal> { animal });
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
        pdfBytes.Length.Should().BeGreaterThan(2000);
    }

    [Fact]
    public void GenerateReport_WithMultipleAnimals_ShouldCreateEvenLargerPdf()
    {
        var animals = new List<Animal>
        {
            CreateTestAnimal("Burek", AnimalSpecies.Dog, AnimalSex.Male),
            CreateTestAnimal("Mruczek", AnimalSpecies.Cat, AnimalSex.Female),
        };
        var reportData = CreateReportDataWithAnimals(animals);
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
        pdfBytes.Length.Should().BeGreaterThan(3000);
    }

    [Fact]
    public void GenerateReport_WithEmptyAnimalsList_ShouldCreateValidPdf()
    {
        var reportData = CreateReportDataWithAnimals(new List<Animal>());
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
    }

    [Fact]
    public void GenerateReport_WithAnimalWithPhotos_ShouldCreatePdfWithoutPhotoSection()
    {
        var animal = CreateTestAnimalWithPhotos("Burek", AnimalSpecies.Dog, AnimalSex.Male);
        var reportData = CreateReportDataWithAnimals(new List<Animal> { animal });
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
    }

    private static RepositoryDumpReportData CreateValidReportData()
    {
        var animal = CreateTestAnimal("Burek", AnimalSpecies.Dog, AnimalSex.Male);
        return CreateReportDataWithAnimals(new List<Animal> { animal });
    }

    private static RepositoryDumpReportData CreateReportDataWithAnimals(List<Animal> animals)
    {
        return new RepositoryDumpReportData
        {
            ShelterId = "test-shelter",
            Animals = animals,
            ReportDate = DateTimeOffset.UtcNow,
            PhotoData = new Dictionary<string, byte[]>(),
        };
    }

    private static Animal CreateTestAnimal(string name, AnimalSpecies species, AnimalSex sex)
    {
        var signature = AnimalSignature.CreateForYear(2024, 1);
        return Animal.Create(
            signature,
            "transponder-001",
            name,
            "czarny",
            species,
            sex,
            new DateTimeOffset(2020, 5, 15, 0, 0, 0, TimeSpan.Zero),
            "test-shelter");
    }

    private static Animal CreateTestAnimalWithPhotos(string name, AnimalSpecies species, AnimalSex sex)
    {
        var animal = CreateTestAnimal(name, species, sex);
        animal.AddPhoto("photos/test.jpg", "test.jpg", true);
        return animal;
    }
}
