using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;
using AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.PdfHelpers;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.ReportPdfs;

public class SelectedAnimalsReportPdfServiceTests
{
    private readonly SelectedAnimalsReportPdfService _pdfService = new();

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
        var reportData = CreateReportDataWithAnimals([animal], [animal.Id]);
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
        pdfBytes.Length.Should().BeGreaterThan(2000);
    }

    [Fact]
    public void GenerateReport_WithMultipleAnimals_ShouldCreateEvenLargerPdf()
    {
        var animal1 = CreateTestAnimal("Burek", AnimalSpecies.Dog, AnimalSex.Male);
        var animal2 = CreateTestAnimal("Mruczek", AnimalSpecies.Cat, AnimalSex.Female);
        var animals = new List<Animal> { animal1, animal2 };
        var reportData = CreateReportDataWithAnimals(animals, animals.Select(a => a.Id).ToList());
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
        pdfBytes.Length.Should().BeGreaterThan(3000);
    }

    [Fact]
    public void GenerateReport_WithEmptyAnimalsList_ShouldCreateValidPdf()
    {
        var reportData = CreateReportDataWithAnimals([], [Guid.NewGuid()]);
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
    }

    [Fact]
    public void GenerateReport_WithAnimalWithPhotos_ShouldCreatePdfWithPhotoSection()
    {
        var animal = CreateTestAnimalWithPhotos("Burek", AnimalSpecies.Dog, AnimalSex.Male);
        var reportData = CreateReportDataWithAnimalsAndPhotos(
            [animal],
            [animal.Id],
            CreateTestPhotoData());
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
        pdfBytes.Length.Should().BeGreaterThan(2000);
    }

    [Fact]
    public void GenerateReport_WithPhotosButNoPhotoData_ShouldCreateValidPdf()
    {
        var animal = CreateTestAnimalWithPhotos("Burek", AnimalSpecies.Dog, AnimalSex.Male);
        var reportData = CreateReportDataWithAnimalsAndPhotos(
            [animal],
            [animal.Id],
            new Dictionary<string, byte[]>());
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
    }

    private static SelectedAnimalsReportData CreateValidReportData()
    {
        var animal = CreateTestAnimal("Burek", AnimalSpecies.Dog, AnimalSex.Male);
        return CreateReportDataWithAnimals([animal], [animal.Id]);
    }

    private static SelectedAnimalsReportData CreateReportDataWithAnimals(List<Animal> animals, List<Guid> requestedIds)
    {
        return new SelectedAnimalsReportData
        {
            ShelterId = "test-shelter",
            Animals = animals,
            RequestedIds = requestedIds,
            ReportDate = DateTimeOffset.UtcNow,
            PhotoData = new Dictionary<string, byte[]>(),
        };
    }

    private static SelectedAnimalsReportData CreateReportDataWithAnimalsAndPhotos(
        List<Animal> animals,
        List<Guid> requestedIds,
        Dictionary<string, byte[]> photoData)
    {
        return new SelectedAnimalsReportData
        {
            ShelterId = "test-shelter",
            Animals = animals,
            RequestedIds = requestedIds,
            ReportDate = DateTimeOffset.UtcNow,
            PhotoData = photoData,
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

    private static Dictionary<string, byte[]> CreateTestPhotoData()
    {
        var jpegBytes = new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
            0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xD9,
        };
        return new Dictionary<string, byte[]> { { "https://example.com/photos/test.jpg", jpegBytes } };
    }
}
