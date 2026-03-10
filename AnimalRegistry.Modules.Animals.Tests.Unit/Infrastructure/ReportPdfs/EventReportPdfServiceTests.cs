using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;
using AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.PdfHelpers;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.ReportPdfs;

public class EventReportPdfServiceTests
{
    private readonly EventReportPdfService _pdfService = new();

    [Fact]
    public void GenerateReport_WithValidData_ShouldCreateValidPdfStructure()
    {
        var reportData = CreateValidReportData();
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
    }

    [Fact]
    public void GenerateReport_WithValidData_ShouldCreateNonEmptyPdf()
    {
        var reportData = CreateValidReportData();
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        pdfBytes.Should().NotBeNullOrEmpty();
        pdfBytes.Length.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void GenerateReport_WithDogSpecies_ShouldCreatePdf()
    {
        var reportData = CreateReportDataWithSpecies(AnimalSpecies.Dog);
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
    }

    [Fact]
    public void GenerateReport_WithCatSpecies_ShouldCreatePdf()
    {
        var reportData = CreateReportDataWithSpecies(AnimalSpecies.Cat);
        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
    }

    [Fact]
    public void GenerateReport_WithEmptyStats_ShouldCreateValidPdf()
    {
        var reportData = new EventReportData
        {
            ShelterId = "test-shelter",
            ReportDate = DateTimeOffset.UtcNow,
            SpeciesStats = new List<SpeciesEventStats>(),
        };

        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
    }

    [Fact]
    public void GenerateReport_WithEventData_ShouldCreateValidPdf()
    {
        var reportData = new EventReportData
        {
            ShelterId = "test-shelter",
            ReportDate = DateTimeOffset.UtcNow,
            SpeciesStats = new List<SpeciesEventStats>
            {
                new()
                {
                    Species = AnimalSpecies.Dog,
                    QuarterStats =
                        new PeriodStats
                        {
                            PeriodFrom = DateTimeOffset.UtcNow.AddDays(-90),
                            PeriodTo = DateTimeOffset.UtcNow,
                            EventCounts =
                                new List<EventTypeCount>
                                {
                                    new() { EventType = AnimalEventType.Adoption, Count = 5 },
                                    new() { EventType = AnimalEventType.Sterilization, Count = 3 },
                                },
                        },
                    MonthStats =
                        new PeriodStats
                        {
                            PeriodFrom = DateTimeOffset.UtcNow.AddDays(-30),
                            PeriodTo = DateTimeOffset.UtcNow,
                            EventCounts = new List<EventTypeCount>(),
                        },
                    WeekStats = new PeriodStats
                    {
                        PeriodFrom = DateTimeOffset.UtcNow.AddDays(-7),
                        PeriodTo = DateTimeOffset.UtcNow,
                        EventCounts = new List<EventTypeCount>(),
                    },
                },
            },
        };

        var pdfBytes = _pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        PdfTestHelpers.AssertValidPdfStructure(pdfBytes);
        pdfBytes.Length.Should().BeGreaterThan(2000);
    }

    private static EventReportData CreateValidReportData()
    {
        return new EventReportData
        {
            ShelterId = "test-shelter",
            ReportDate = DateTimeOffset.UtcNow,
            SpeciesStats = new List<SpeciesEventStats>
            {
                new()
                {
                    Species = AnimalSpecies.Dog,
                    QuarterStats =
                        new PeriodStats
                        {
                            PeriodFrom = DateTimeOffset.UtcNow.AddDays(-90),
                            PeriodTo = DateTimeOffset.UtcNow,
                            EventCounts =
                                new List<EventTypeCount>
                                {
                                    new() { EventType = AnimalEventType.Adoption, Count = 5 },
                                },
                        },
                    MonthStats =
                        new PeriodStats
                        {
                            PeriodFrom = DateTimeOffset.UtcNow.AddDays(-30),
                            PeriodTo = DateTimeOffset.UtcNow,
                            EventCounts = new List<EventTypeCount>(),
                        },
                    WeekStats = new PeriodStats
                    {
                        PeriodFrom = DateTimeOffset.UtcNow.AddDays(-7),
                        PeriodTo = DateTimeOffset.UtcNow,
                        EventCounts = new List<EventTypeCount>(),
                    },
                },
            },
        };
    }

    private static EventReportData CreateReportDataWithSpecies(AnimalSpecies species)
    {
        return new EventReportData
        {
            ShelterId = "test-shelter",
            ReportDate = DateTimeOffset.UtcNow,
            SpeciesStats = new List<SpeciesEventStats>
            {
                new()
                {
                    Species = species,
                    QuarterStats =
                        new PeriodStats
                        {
                            PeriodFrom = DateTimeOffset.UtcNow.AddDays(-90),
                            PeriodTo = DateTimeOffset.UtcNow,
                            EventCounts = new List<EventTypeCount>(),
                        },
                    MonthStats =
                        new PeriodStats
                        {
                            PeriodFrom = DateTimeOffset.UtcNow.AddDays(-30),
                            PeriodTo = DateTimeOffset.UtcNow,
                            EventCounts = new List<EventTypeCount>(),
                        },
                    WeekStats = new PeriodStats
                    {
                        PeriodFrom = DateTimeOffset.UtcNow.AddDays(-7),
                        PeriodTo = DateTimeOffset.UtcNow,
                        EventCounts = new List<EventTypeCount>(),
                    },
                },
            },
        };
    }
}