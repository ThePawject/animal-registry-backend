using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application.Origin;

public class AnimalOriginLookupTests
{
    private static Animal CreateAnimal()
    {
        return Animal.Create(AnimalSignature.Create("2026/0001").Value!, null, "Burek", "Brown", "Mix", "",
            AnimalSpecies.Dog, AnimalSex.Male, null, "shelter");
    }

    [Fact]
    public void Resolve_ShouldReturnUnassigned_WhenAnimalHasNoOrigin()
    {
        var lookup = new AnimalOriginLookup(new Dictionary<Guid, string>());

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
        var foundId = Guid.NewGuid();
        var contractorId = Guid.NewGuid();
        var animal = CreateAnimal();
        animal.SetOrigin(AnimalOrigin.Create(DateTimeOffset.UtcNow, AdmissionMethod.BroughtByPrivatePerson,
            "Leśna 1", foundId, contractorId));
        var lookup = new AnimalOriginLookup(new Dictionary<Guid, string>
        {
            [foundId] = "Gmina A", [contractorId] = "Gmina B",
        });

        var origin = lookup.Resolve(animal);

        origin.IsAssigned.Should().BeTrue();
        origin.AdmissionMethodName.Should().Be("Doprowadzony przez osobę fizyczną");
        origin.FoundMunicipality.Should().Be(new MunicipalityReferenceDto(foundId, "Gmina A"));
        origin.ContractorMunicipality.Should().Be(new MunicipalityReferenceDto(contractorId, "Gmina B"));
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
