using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Shared.Access;
using FluentAssertions;
using NSubstitute;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application.Reports;

public class GenerateEventReportCommandHandlerTests
{
    private const string TestShelterId = "test-shelter-id";

    private static ICurrentUser CreateCurrentUserMock()
    {
        var currentUserMock = Substitute.For<ICurrentUser>();
        currentUserMock.ShelterId.Returns(TestShelterId);
        return currentUserMock;
    }

    [Fact]
    public async Task Handle_ShouldGenerateReport_WithEmptyEvents()
    {
        var repoMock = Substitute.For<IAnimalEventRepository>();
        repoMock.GetAllByShelterIdAsync(TestShelterId, Arg.Any<CancellationToken>())
            .Returns([]);

        var currentUserMock = CreateCurrentUserMock();
        var pdfServiceMock = Substitute.For<IEventReportPdfService>();
        pdfServiceMock.GenerateReport(Arg.Any<EventReportData>(), Arg.Any<DateTimeOffset>())
            .Returns("%PDF"u8.ToArray());

        var handler = new GenerateEventReportCommandHandler(repoMock, currentUserMock, pdfServiceMock);

        var result = await handler.Handle(new GenerateEventReportCommand(), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FileName.Should().StartWith("RaportZdarzen_");
        result.Value.ContentType.Should().Be("application/pdf");
        result.Value.Data.Should().NotBeNullOrEmpty();

        await repoMock.Received(1).GetAllByShelterIdAsync(TestShelterId, Arg.Any<CancellationToken>());
        pdfServiceMock.Received(1).GenerateReport(Arg.Any<EventReportData>(), Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task Handle_ShouldGenerateReport_WithDogAndCatEvents()
    {
        var now = DateTimeOffset.UtcNow;
        var events = new List<AnimalEventWithAnimalInfo>
        {
            new(AnimalEvent.Create(AnimalEventType.Adoption, now.AddDays(-5), "Adopted", "User1"), AnimalSpecies.Dog,
                Guid.NewGuid(), "Dog1"),
            new(AnimalEvent.Create(AnimalEventType.Adoption, now.AddDays(-3), "Adopted", "User1"), AnimalSpecies.Cat,
                Guid.NewGuid(), "Cat1"),
            new(AnimalEvent.Create(AnimalEventType.Sterilization, now.AddDays(-10), "Sterilized", "User2"),
                AnimalSpecies.Dog, Guid.NewGuid(), "Dog2"),
            new(AnimalEvent.Create(AnimalEventType.RabiesVaccination, now.AddDays(-20), "Vaccinated", "User3"),
                AnimalSpecies.Cat, Guid.NewGuid(), "Cat2"),
        };

        var repoMock = Substitute.For<IAnimalEventRepository>();
        repoMock.GetAllByShelterIdAsync(TestShelterId, Arg.Any<CancellationToken>())
            .Returns(events);

        var currentUserMock = CreateCurrentUserMock();
        var pdfServiceMock = Substitute.For<IEventReportPdfService>();
        pdfServiceMock.GenerateReport(Arg.Any<EventReportData>(), Arg.Any<DateTimeOffset>())
            .Returns("%PDF"u8.ToArray());

        var handler = new GenerateEventReportCommandHandler(repoMock, currentUserMock, pdfServiceMock);

        var result = await handler.Handle(new GenerateEventReportCommand(), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        pdfServiceMock.Received(1).GenerateReport(
            Arg.Is<EventReportData>(data =>
                data.ShelterId == TestShelterId &&
                data.SpeciesStats.Count == 2 &&
                data.SpeciesStats.Any(s => s.Species == AnimalSpecies.Dog) &&
                data.SpeciesStats.Any(s => s.Species == AnimalSpecies.Cat)),
            Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task Handle_ShouldAggregateEventCounts_ByTypeAndPeriod()
    {
        var now = DateTimeOffset.UtcNow;
        var events = new List<AnimalEventWithAnimalInfo>
        {
            new(AnimalEvent.Create(AnimalEventType.Adoption, now.AddDays(-5), "Adopted", "User1"),
                AnimalSpecies.Dog, Guid.NewGuid(), "Dog1"),
            new(AnimalEvent.Create(AnimalEventType.Adoption, now.AddDays(-3), "Adopted", "User1"),
                AnimalSpecies.Dog, Guid.NewGuid(), "Dog2"),
            new(AnimalEvent.Create(AnimalEventType.Adoption, now.AddDays(-40), "Adopted", "User1"),
                AnimalSpecies.Dog, Guid.NewGuid(), "Dog3"),
        };

        var repoMock = Substitute.For<IAnimalEventRepository>();
        repoMock.GetAllByShelterIdAsync(TestShelterId, Arg.Any<CancellationToken>())
            .Returns(events);

        var currentUserMock = CreateCurrentUserMock();
        var pdfServiceMock = Substitute.For<IEventReportPdfService>();
        pdfServiceMock.GenerateReport(Arg.Any<EventReportData>(), Arg.Any<DateTimeOffset>())
            .Returns("%PDF"u8.ToArray());

        var handler = new GenerateEventReportCommandHandler(repoMock, currentUserMock, pdfServiceMock);

        await handler.Handle(new GenerateEventReportCommand(), CancellationToken.None);

        var receivedData = pdfServiceMock.ReceivedCalls()
            .First(c => c.GetMethodInfo().Name == "GenerateReport")
            .GetArguments()[0] as EventReportData;

        receivedData.Should().NotBeNull();
        var dogStats = receivedData.SpeciesStats.First(s => s.Species == AnimalSpecies.Dog);
        var weekAdoptions = dogStats.PeriodStats.First(s => s.Title == "Okres tygodniowy").EventCounts
            .FirstOrDefault(e => e.EventType == AnimalEventType.Adoption);
        var quarterAdoptions =
            dogStats.PeriodStats.First(s => s.Title == "Okres kwartalny").EventCounts
                .FirstOrDefault(e => e.EventType == AnimalEventType.Adoption);

        weekAdoptions.Should().NotBeNull();
        weekAdoptions.Count.Should().Be(2);
        quarterAdoptions.Should().NotBeNull();
        quarterAdoptions.Count.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldGenerateOnlySelectedPeriods()
    {
        var repoMock = Substitute.For<IAnimalEventRepository>();
        repoMock.GetAllByShelterIdAsync(TestShelterId, Arg.Any<CancellationToken>())
            .Returns([]);

        var currentUserMock = CreateCurrentUserMock();
        var pdfServiceMock = Substitute.For<IEventReportPdfService>();
        pdfServiceMock.GenerateReport(Arg.Any<EventReportData>(), Arg.Any<DateTimeOffset>())
            .Returns("%PDF"u8.ToArray());

        var handler = new GenerateEventReportCommandHandler(repoMock, currentUserMock, pdfServiceMock);

        await handler.Handle(new GenerateEventReportCommand { Periods = [EventReportPeriod.Week] }, CancellationToken.None);

        var receivedData = pdfServiceMock.ReceivedCalls()
            .First(c => c.GetMethodInfo().Name == "GenerateReport")
            .GetArguments()[0] as EventReportData;

        receivedData.Should().NotBeNull();
        receivedData!.SpeciesStats.Should().AllSatisfy(stats =>
        {
            stats.PeriodStats.Should().ContainSingle();
            stats.PeriodStats[0].Title.Should().Be("Okres tygodniowy");
        });
    }

    [Fact]
    public async Task Handle_ShouldAggregateCustomPeriod()
    {
        var now = DateTimeOffset.UtcNow;
        var customStart = now.AddDays(-50);
        var customEnd = now.AddDays(-20);
        var events = new List<AnimalEventWithAnimalInfo>
        {
            new(AnimalEvent.Create(AnimalEventType.Adoption, now.AddDays(-45), "Adopted", "User1"),
                AnimalSpecies.Dog, Guid.NewGuid(), "Dog1"),
            new(AnimalEvent.Create(AnimalEventType.Adoption, now.AddDays(-10), "Adopted", "User1"),
                AnimalSpecies.Dog, Guid.NewGuid(), "Dog2"),
        };

        var repoMock = Substitute.For<IAnimalEventRepository>();
        repoMock.GetAllByShelterIdAsync(TestShelterId, Arg.Any<CancellationToken>())
            .Returns(events);

        var currentUserMock = CreateCurrentUserMock();
        var pdfServiceMock = Substitute.For<IEventReportPdfService>();
        pdfServiceMock.GenerateReport(Arg.Any<EventReportData>(), Arg.Any<DateTimeOffset>())
            .Returns("%PDF"u8.ToArray());

        var handler = new GenerateEventReportCommandHandler(repoMock, currentUserMock, pdfServiceMock);

        await handler.Handle(new GenerateEventReportCommand
        {
            Periods = [EventReportPeriod.Custom],
            CustomStartDate = customStart,
            CustomEndDate = customEnd,
        }, CancellationToken.None);

        var receivedData = pdfServiceMock.ReceivedCalls()
            .First(c => c.GetMethodInfo().Name == "GenerateReport")
            .GetArguments()[0] as EventReportData;

        var dogStats = receivedData!.SpeciesStats.First(s => s.Species == AnimalSpecies.Dog);
        var customStats = dogStats.PeriodStats.Single();

        customStats.Title.Should().Be("Własny zakres");
        customStats.EventCounts.Should().ContainSingle(e =>
            e.EventType == AnimalEventType.Adoption && e.Count == 1);
    }
}
