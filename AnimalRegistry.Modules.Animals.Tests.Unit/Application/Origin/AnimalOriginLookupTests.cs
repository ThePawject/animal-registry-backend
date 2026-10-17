using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using FluentAssertions;
using static AnimalRegistry.Modules.Animals.Tests.Unit.OriginTestData;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application.Origin;

public class AnimalOriginLookupTests
{
    [Fact]
    public void Resolve_ShouldReturnUnassigned_WhenAnimalHasNoOrigin()
    {
        var lookup = new AnimalOriginLookup(new Dictionary<Guid, AnimalOrigin>(), new Dictionary<Guid, string>());

        var origin = lookup.Resolve(CreateAnimal());

        origin.Should().Be(AnimalOriginDto.Unassigned);
        origin.IsAssigned.Should().BeFalse();
        origin.AdmissionMethodName.Should().Be("Brak przypisania");
        origin.FoundStreet.Should().Be("Brak przypisania");
        origin.FoundMunicipality.Should().Be(new MunicipalityReferenceDto(null, "Brak przypisania"));
        origin.ContractorMunicipality.Should().Be(new MunicipalityReferenceDto(null, "Brak przypisania"));
    }

    [Fact]
    public void Resolve_ShouldMapOriginWithMunicipalityNames()
    {
        var found = CreateMunicipality("Gmina A");
        var contractor = CreateMunicipality("Gmina B");
        var animal = CreateAnimal();
        var stored = CreateOrigin(animal, found, contractor);
        var lookup = new AnimalOriginLookup(
            new Dictionary<Guid, AnimalOrigin> { [animal.Id] = stored },
            new Dictionary<Guid, string> { [found.Id] = "Gmina A", [contractor.Id] = "Gmina B" });

        var origin = lookup.Resolve(animal);

        origin.IsAssigned.Should().BeTrue();
        origin.AdmissionDate.Should().Be(AdmissionDate);
        origin.AdmissionMethodName.Should().Be("Podrzucony");
        origin.FoundMunicipality.Should().Be(new MunicipalityReferenceDto(found.Id, "Gmina A"));
        origin.ContractorMunicipality.Should().Be(new MunicipalityReferenceDto(contractor.Id, "Gmina B"));
    }

    [Fact]
    public void Resolve_ShouldReturnUnassignedMunicipality_WhenNameIsNotLoaded()
    {
        var found = CreateMunicipality("Gmina A");
        var animal = CreateAnimal();
        var lookup = new AnimalOriginLookup(
            new Dictionary<Guid, AnimalOrigin> { [animal.Id] = CreateOrigin(animal, found, found) },
            new Dictionary<Guid, string>());

        lookup.Resolve(animal).FoundMunicipality.Should().Be(MunicipalityReferenceDto.Unassigned);
    }

    [Fact]
    public void AdmissionMethodNames_ShouldBeDefinedForEveryMethod()
    {
        foreach (var method in Enum.GetValues<AdmissionMethod>())
        {
            OriginLabels.GetAdmissionMethodName(method).Should().NotBe(method.ToString());
        }
    }
}
