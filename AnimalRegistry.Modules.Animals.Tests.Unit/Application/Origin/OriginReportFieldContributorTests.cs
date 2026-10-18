using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using FluentAssertions;
using NSubstitute;
using static AnimalRegistry.Modules.Animals.Tests.Unit.OriginTestData;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application.Origin;

public class OriginReportFieldContributorTests
{
    private readonly IAnimalOriginReader _reader = Substitute.For<IAnimalOriginReader>();

    private void GivenLookup(AnimalOriginLookup? lookup)
    {
        _reader.LoadAsync(Arg.Any<IReadOnlyCollection<Animal>>(), Arg.Any<CancellationToken>()).Returns(lookup);
    }

    [Fact]
    public async Task GetFieldsAsync_ShouldReturnNoFields_WhenModuleIsDisabled()
    {
        GivenLookup(null);

        var fields = await new OriginReportFieldContributor(_reader).GetFieldsAsync([CreateAnimal()], default);

        fields.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFieldsAsync_ShouldFormatOriginWithAdmissionDateInPolishTime()
    {
        var municipality = CreateMunicipality("Gmina A");
        var animal = CreateAnimal();
        AssignOrigin(animal, municipality, municipality, new DateTimeOffset(2026, 3, 31, 23, 30, 0, TimeSpan.Zero));
        GivenLookup(new AnimalOriginLookup(new Dictionary<Guid, string> { [municipality.Id] = "Gmina A" }));

        var fields = await new OriginReportFieldContributor(_reader).GetFieldsAsync([animal], default);

        fields.ToDictionary(f => f.Label, f => f.Format(animal)).Should().Equal(new Dictionary<string, string>
        {
            ["Data przyjęcia"] = "01.04.2026",
            ["Sposób trafienia"] = "Podrzucony",
            ["Ulica znalezienia"] = "Leśna 1",
            ["Gmina znalezienia"] = "Gmina A",
            ["Gmina rozliczeniowa"] = "Gmina A",
        });
    }

    [Fact]
    public async Task GetFieldsAsync_ShouldReturnUnassigned_WhenAnimalHasNoOrigin()
    {
        var animal = CreateAnimal();
        GivenLookup(new AnimalOriginLookup(new Dictionary<Guid, string>()));

        var fields = await new OriginReportFieldContributor(_reader).GetFieldsAsync([animal], default);

        fields.Select(f => f.Format(animal)).Should().AllBe(OriginLabels.Unassigned);
    }
}
