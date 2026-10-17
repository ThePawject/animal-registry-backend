using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using FluentAssertions;
using NSubstitute;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application.Origin;

public class OriginReportFieldContributorTests
{
    private readonly IAnimalOriginReader _reader = Substitute.For<IAnimalOriginReader>();

    private static Animal CreateAnimal()
    {
        return Animal.Create(AnimalSignature.Create("2026/0001").Value!, null, "Burek", "Brown", "Mix", "",
            AnimalSpecies.Dog, AnimalSex.Male, null, "shelter");
    }

    [Fact]
    public async Task GetFieldsAsync_ShouldReturnNoFields_WhenModuleIsDisabled()
    {
        _reader.LoadAsync(Arg.Any<IReadOnlyCollection<Animal>>(), Arg.Any<CancellationToken>())
            .Returns((AnimalOriginLookup?)null);

        var fields = await new OriginReportFieldContributor(_reader).GetFieldsAsync([CreateAnimal()], default);

        fields.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFieldsAsync_ShouldFormatAdmissionDateInPolishTime()
    {
        var animal = CreateAnimal();
        animal.SetOrigin(AnimalOrigin.Create(new DateTimeOffset(2026, 3, 31, 23, 30, 0, TimeSpan.Zero),
            AdmissionMethod.Abandoned, "Leśna 1", Guid.NewGuid(), Guid.NewGuid()));
        _reader.LoadAsync(Arg.Any<IReadOnlyCollection<Animal>>(), Arg.Any<CancellationToken>())
            .Returns(new AnimalOriginLookup(new Dictionary<Guid, string>()));

        var fields = await new OriginReportFieldContributor(_reader).GetFieldsAsync([animal], default);

        fields.Single(f => f.Label == "Data przyjęcia").Format(animal).Should().Be("01.04.2026");
    }

    [Fact]
    public async Task GetFieldsAsync_ShouldReturnUnassigned_WhenAnimalHasNoOrigin()
    {
        var animal = CreateAnimal();
        _reader.LoadAsync(Arg.Any<IReadOnlyCollection<Animal>>(), Arg.Any<CancellationToken>())
            .Returns(new AnimalOriginLookup(new Dictionary<Guid, string>()));

        var fields = await new OriginReportFieldContributor(_reader).GetFieldsAsync([animal], default);

        fields.Select(f => f.Format(animal)).Should().AllBe(OriginLabels.Unassigned);
    }
}
