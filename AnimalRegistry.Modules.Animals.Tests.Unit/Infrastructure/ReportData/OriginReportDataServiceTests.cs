using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.ReportData;
using FluentAssertions;
using NSubstitute;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.ReportData;

public class OriginReportDataServiceTests
{
    private const string ShelterId = "test-shelter-id";
    private static readonly Guid Zabki = Guid.NewGuid();
    private static readonly Guid Lomianki = Guid.NewGuid();

    private readonly IAnimalRepository _animals = Substitute.For<IAnimalRepository>();
    private readonly IAnimalOriginReader _originReader = Substitute.For<IAnimalOriginReader>();
    private readonly OriginReportDataService _service;

    public OriginReportDataServiceTests()
    {
        _originReader.LoadAsync(Arg.Any<IReadOnlyCollection<Animal>>(), Arg.Any<CancellationToken>())
            .Returns(new AnimalOriginLookup(new Dictionary<Guid, string>
            {
                [Zabki] = "Ząbki", [Lomianki] = "Łomianki",
            }));
        _service = new OriginReportDataService(_animals, _originReader);
    }

    private static Animal CreateAnimal(string signature, Guid? contractorId, DateTimeOffset? admittedOn = null)
    {
        var animal = Animal.Create(AnimalSignature.Create(signature).Value!, null, "Burek", "Brown", "Mix", "",
            AnimalSpecies.Dog, AnimalSex.Male, null, ShelterId);
        if (contractorId.HasValue)
        {
            animal.SetOrigin(AnimalOrigin.Create(admittedOn!.Value, AdmissionMethod.Abandoned, "Leśna 1",
                contractorId.Value, contractorId.Value));
        }

        return animal;
    }

    private void GivenAnimals(params Animal[] animals)
    {
        _animals.GetAdmittedBetweenAsync(ShelterId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>())
            .Returns(animals);
    }

    [Fact]
    public async Task PrepareMonthlyReportDataAsync_ShouldGroupByContractor_WithUnassignedLast()
    {
        var march = new DateTimeOffset(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);
        GivenAnimals(
            CreateAnimal("2026/0001", null),
            CreateAnimal("2026/0002", Zabki, march),
            CreateAnimal("2026/0003", Lomianki, march),
            CreateAnimal("2026/0004", Zabki, march.AddDays(1)));

        var data = await _service.PrepareMonthlyReportDataAsync(ShelterId, 2026, 3);

        data.Groups.Select(g => g.ContractorName).Should().Equal("Łomianki", "Ząbki", "Brak przypisania");
        data.Groups[1].Entries.Select(e => e.Animal.Signature.Value).Should().Equal("2026/0002", "2026/0004");
        data.Groups[2].IsUnassigned.Should().BeTrue();
        data.TotalAnimals.Should().Be(4);
    }

    [Fact]
    public async Task PrepareMonthlyReportDataAsync_ShouldQueryPolishCalendarMonth()
    {
        GivenAnimals();

        await _service.PrepareMonthlyReportDataAsync(ShelterId, 2026, 3);

        await _animals.Received(1).GetAdmittedBetweenAsync(
            ShelterId,
            new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.FromHours(1)),
            new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.FromHours(2)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PrepareYearlyReportDataAsync_ShouldCountAnimalsPerContractorAndLocalMonth()
    {
        GivenAnimals(
            CreateAnimal("2026/0001", Zabki, new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero)),
            CreateAnimal("2026/0002", Zabki, new DateTimeOffset(2026, 1, 20, 12, 0, 0, TimeSpan.Zero)),
            CreateAnimal("2026/0003", Lomianki, new DateTimeOffset(2026, 1, 31, 23, 30, 0, TimeSpan.Zero)));

        var data = await _service.PrepareYearlyReportDataAsync(ShelterId, 2026);

        var zabki = data.Rows.Single(r => r.ContractorName == "Ząbki");
        var lomianki = data.Rows.Single(r => r.ContractorName == "Łomianki");
        zabki.MonthCounts[0].Should().Be(2);
        lomianki.MonthCounts[1].Should().Be(1);
        data.MonthTotals[0].Should().Be(2);
        data.Total.Should().Be(3);
    }
}
