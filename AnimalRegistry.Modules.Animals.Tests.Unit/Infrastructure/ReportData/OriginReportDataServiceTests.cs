using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.ReportData;
using FluentAssertions;
using NSubstitute;
using static AnimalRegistry.Modules.Animals.Tests.Unit.OriginTestData;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure.ReportData;

public class OriginReportDataServiceTests
{
    private static readonly Municipality Zabki = CreateMunicipality("Ząbki");
    private static readonly Municipality Lomianki = CreateMunicipality("Łomianki");

    private readonly IOriginReportQuery _query = Substitute.For<IOriginReportQuery>();
    private readonly IMunicipalityRepository _municipalities = Substitute.For<IMunicipalityRepository>();
    private readonly OriginReportDataService _service;

    public OriginReportDataServiceTests()
    {
        _municipalities.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), ShelterId, Arg.Any<CancellationToken>())
            .Returns([Zabki, Lomianki]);
        _service = new OriginReportDataService(_query, _municipalities);
    }

    private static AdmittedAnimal AdmittedAnimal(string signature, Municipality? contractor,
        DateTimeOffset? admittedOn = null)
    {
        var animal = CreateAnimal(signature);
        if (contractor is null)
        {
            return new AdmittedAnimal(animal, animal.CreatedOn);
        }

        var origin = AssignOrigin(animal, contractor, contractor, admittedOn);
        return new AdmittedAnimal(animal, origin.AdmissionDate);
    }

    private void GivenAnimals(params AdmittedAnimal[] animals)
    {
        _query.GetAdmittedBetweenAsync(ShelterId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>())
            .Returns(animals);
    }

    [Fact]
    public async Task PrepareMonthlyReportDataAsync_ShouldGroupByContractor_WithUnassignedLast()
    {
        var march = new DateTimeOffset(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);
        GivenAnimals(
            AdmittedAnimal("2026/0001", null),
            AdmittedAnimal("2026/0002", Zabki, march),
            AdmittedAnimal("2026/0003", Lomianki, march),
            AdmittedAnimal("2026/0004", Zabki, march.AddDays(1)));

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

        await _query.Received(1).GetAdmittedBetweenAsync(
            ShelterId,
            new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.FromHours(1)),
            new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.FromHours(2)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PrepareYearlyReportDataAsync_ShouldCountAnimalsPerContractorAndLocalMonth()
    {
        GivenAnimals(
            AdmittedAnimal("2026/0001", Zabki, new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero)),
            AdmittedAnimal("2026/0002", Zabki, new DateTimeOffset(2026, 1, 20, 12, 0, 0, TimeSpan.Zero)),
            AdmittedAnimal("2026/0003", Lomianki, new DateTimeOffset(2026, 1, 31, 23, 30, 0, TimeSpan.Zero)));

        var data = await _service.PrepareYearlyReportDataAsync(ShelterId, 2026);

        var zabki = data.Rows.Single(r => r.ContractorName == "Ząbki");
        var lomianki = data.Rows.Single(r => r.ContractorName == "Łomianki");
        zabki.MonthCounts[0].Should().Be(2);
        lomianki.MonthCounts[1].Should().Be(1);
        data.MonthTotals[0].Should().Be(2);
        data.Total.Should().Be(3);
    }
}
