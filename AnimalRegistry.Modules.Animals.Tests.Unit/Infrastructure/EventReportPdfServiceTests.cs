using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Modules.Animals.Infrastructure.Services;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure;

public class EventReportPdfServiceTests
{
    [Fact]
    public void GenerateReport_ShouldCreateValidPdfStructure()
    {
        var pdfService = new EventReportPdfService();
        
        var reportData = new EventReportData
        {
            ShelterId = "test-shelter",
            ReportDate = DateTimeOffset.UtcNow,
            SpeciesStats = new List<SpeciesEventStats>
            {
                new()
                {
                    Species = AnimalSpecies.Dog,
                    QuarterStats = new PeriodStats
                    {
                        PeriodFrom = DateTimeOffset.UtcNow.AddDays(-90),
                        PeriodTo = DateTimeOffset.UtcNow,
                        EventCounts = new List<EventTypeCount>
                        {
                            new() { EventType = AnimalEventType.Adoption, Count = 5 }
                        }
                    },
                    MonthStats = new PeriodStats
                    {
                        PeriodFrom = DateTimeOffset.UtcNow.AddDays(-30),
                        PeriodTo = DateTimeOffset.UtcNow,
                        EventCounts = new List<EventTypeCount>()
                    },
                    WeekStats = new PeriodStats
                    {
                        PeriodFrom = DateTimeOffset.UtcNow.AddDays(-7),
                        PeriodTo = DateTimeOffset.UtcNow,
                        EventCounts = new List<EventTypeCount>()
                    }
                }
            }
        };
        
        var pdfBytes = pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        
        pdfBytes.Should().NotBeNullOrEmpty();
        pdfBytes.Length.Should().BeGreaterThan(1000);
        
        pdfBytes[0].Should().Be(0x25);
        pdfBytes[1].Should().Be(0x50);
        pdfBytes[2].Should().Be(0x44);
        pdfBytes[3].Should().Be(0x46);
        
        var pdfContent = System.Text.Encoding.ASCII.GetString(pdfBytes);
        pdfContent.Should().Contain("%PDF-");
        pdfContent.Should().Contain("%%EOF");
        pdfContent.Should().Contain("/Type /Catalog");
        pdfContent.Should().Contain("/Type /Pages");
    }
    
    [Fact]
    public void GenerateReport_ShouldNotContainPlaceholderText()
    {
        var pdfService = new EventReportPdfService();
        
        var reportData = new EventReportData
        {
            ShelterId = "ABC-SHELTER",
            ReportDate = new DateTimeOffset(2025, 1, 15, 0, 0, 0, TimeSpan.Zero),
            SpeciesStats = new List<SpeciesEventStats>()
        };
        
        var pdfBytes = pdfService.GenerateReport(reportData, new DateTimeOffset(2025, 1, 15, 0, 0, 0, TimeSpan.Zero));
        
        pdfBytes.Should().NotBeNullOrEmpty();
        pdfBytes.Length.Should().BeGreaterThan(1000);
        
        File.WriteAllBytes("/tmp/test_minimal.pdf", pdfBytes);
        
        var pdfContent = System.Text.Encoding.ASCII.GetString(pdfBytes);
        
        pdfContent.Should().NotContain("TTTTTT");
    }
    
    [Fact]
    public void GenerateReport_ShouldContainPdfObjects()
    {
        var pdfService = new EventReportPdfService();
        
        var reportData = new EventReportData
        {
            ShelterId = "test",
            ReportDate = DateTimeOffset.UtcNow,
            SpeciesStats = new List<SpeciesEventStats>()
        };
        
        var pdfBytes = pdfService.GenerateReport(reportData, DateTimeOffset.UtcNow);
        var pdfContent = System.Text.Encoding.ASCII.GetString(pdfBytes);
        
        pdfContent.Should().Contain("/Length");
        pdfContent.Should().Contain("stream");
        pdfContent.Should().Contain("endstream");
    }
}