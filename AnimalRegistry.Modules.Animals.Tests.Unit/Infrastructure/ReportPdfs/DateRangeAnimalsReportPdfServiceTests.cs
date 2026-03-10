using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;
using AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.PdfHelpers;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.ReportPdfs;

public class DateRangeAnimalsReportPdfServiceTests
{
    private readonly DateRangeAnimalsReportPdfService _pdfService = new();

    [Fact]
    public void GenerateReport_WithValidData_ShouldCreateValidPdfStructure()
    {
        var reportData = CreateValidReportData();
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
    }

    [Fact]
    public void GenerateReport_WithDateRange_ShouldCreateValidPdf()
    {
        var startDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var endDate = new DateTimeOffset(2025, 1, 31, 0, 0, 0, TimeSpan.Zero);
        var reportData = CreateReportDataWithDateRange(startDate, endDate);
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
    }

    [Fact]
    public void GenerateReport_WithAnimal_ShouldCreateLargerPdf()
    {
        var animal = CreateTestAnimal("Burek", AnimalSpecies.Dog, AnimalSex.Male);
        var reportData = CreateReportDataWithAnimals(new List<AnimalWithFilteredEvents>
        {
            new() { Animal = animal, Events = new List<AnimalEvent>() },
        });
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
        pdfBytes.Length.Should().BeGreaterThan(2000);
    }

    [Fact]
    public void GenerateReport_WithAnimalAndEvents_ShouldCreateEvenLargerPdf()
    {
        var animal = CreateTestAnimal("Burek", AnimalSpecies.Dog, AnimalSex.Male);
        var animalEvent = AnimalEvent.Create(
            AnimalEventType.Adoption,
            new DateTimeOffset(2025, 1, 15, 0, 0, 0, TimeSpan.Zero),
            "Adoptowany przez rodzinę",
            "Jan Kowalski");

        var reportData = CreateReportDataWithAnimals(new List<AnimalWithFilteredEvents>
        {
            new() { Animal = animal, Events = new List<AnimalEvent> { animalEvent } },
        });
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
        pdfBytes.Length.Should().BeGreaterThan(3000);
    }

    [Fact]
    public void GenerateReport_WithEmptyAnimalsList_ShouldCreateValidPdf()
    {
        var reportData = CreateReportDataWithAnimals(new List<AnimalWithFilteredEvents>());
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
    }

    [Fact]
    public void GenerateReport_WithMultipleAnimals_ShouldCreateLargerPdf()
    {
        var animal1 = CreateTestAnimal("Burek", AnimalSpecies.Dog, AnimalSex.Male);
        var animal2 = CreateTestAnimal("Mruczek", AnimalSpecies.Cat, AnimalSex.Female);

        var reportData = CreateReportDataWithAnimals(new List<AnimalWithFilteredEvents>
        {
            new() { Animal = animal1, Events = new List<AnimalEvent>() },
            new() { Animal = animal2, Events = new List<AnimalEvent>() },
        });
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
        pdfBytes.Length.Should().BeGreaterThan(4000);
    }

    private static DateRangeAnimalsReportData CreateValidReportData()
    {
        var animal = CreateTestAnimal("Burek", AnimalSpecies.Dog, AnimalSex.Male);
        return CreateReportDataWithAnimals(new List<AnimalWithFilteredEvents>
        {
            new() { Animal = animal, Events = new List<AnimalEvent>() },
        });
    }

    private static DateRangeAnimalsReportData CreateReportDataWithDateRange(DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        var animal = CreateTestAnimal("Burek", AnimalSpecies.Dog, AnimalSex.Male);
        return new DateRangeAnimalsReportData
        {
            ShelterId = "test-shelter",
            StartDate = startDate,
            EndDate = endDate,
            Animals = new List<AnimalWithFilteredEvents>
            {
                new() { Animal = animal, Events = new List<AnimalEvent>() },
            },
            ReportDate = DateTimeOffset.UtcNow,
        };
    }

    private static DateRangeAnimalsReportData CreateReportDataWithAnimals(List<AnimalWithFilteredEvents> animals)
    {
        return new DateRangeAnimalsReportData
        {
            ShelterId = "test-shelter",
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 1, 31, 0, 0, 0, TimeSpan.Zero),
            Animals = animals,
            ReportDate = DateTimeOffset.UtcNow,
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
}