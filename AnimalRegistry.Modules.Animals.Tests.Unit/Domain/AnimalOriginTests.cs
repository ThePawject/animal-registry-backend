using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Shared.DDD;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Domain;

public class AnimalOriginTests
{
    private static readonly DateTimeOffset AdmissionDate = new(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);
    private static readonly Guid FoundId = Guid.NewGuid();
    private static readonly Guid ContractorId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldTrimStreet_WhenDataIsValid()
    {
        var origin = AnimalOrigin.Create(AdmissionDate, AdmissionMethod.Abandoned, "  Leśna 1 ", FoundId,
            ContractorId);

        origin.AdmissionDate.Should().Be(AdmissionDate);
        origin.AdmissionMethod.Should().Be(AdmissionMethod.Abandoned);
        origin.FoundStreet.Should().Be("Leśna 1");
        origin.FoundMunicipalityId.Should().Be(FoundId);
        origin.ContractorMunicipalityId.Should().Be(ContractorId);
    }

    [Theory]
    [InlineData(AdmissionMethod.Unassigned)]
    [InlineData((AdmissionMethod)999)]
    public void Create_ShouldThrow_WhenAdmissionMethodIsNotAssigned(AdmissionMethod method)
    {
        var act = () => AnimalOrigin.Create(AdmissionDate, method, "Leśna 1", FoundId, ContractorId);

        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenStreetIsMissing(string street)
    {
        var act = () => AnimalOrigin.Create(AdmissionDate, AdmissionMethod.Abandoned, street, FoundId, ContractorId);

        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenMunicipalityIsMissing()
    {
        var withoutFound = () =>
            AnimalOrigin.Create(AdmissionDate, AdmissionMethod.Abandoned, "Leśna 1", Guid.Empty, ContractorId);
        var withoutContractor = () =>
            AnimalOrigin.Create(AdmissionDate, AdmissionMethod.Abandoned, "Leśna 1", FoundId, Guid.Empty);

        withoutFound.Should().Throw<BusinessRuleValidationException>();
        withoutContractor.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void SetOrigin_ShouldAssignOriginAndTouchModifiedOn()
    {
        var animal = Animal.Create(AnimalSignature.Create("2026/0001").Value!, null, "Burek", "Brown", "Mix", "",
            AnimalSpecies.Dog, AnimalSex.Male, null, "shelter");
        var modifiedBefore = animal.ModifiedOn;
        var origin = AnimalOrigin.Create(AdmissionDate, AdmissionMethod.Intervention, "Leśna 1", FoundId,
            ContractorId);

        animal.SetOrigin(origin);

        animal.Origin.Should().BeSameAs(origin);
        animal.ModifiedOn.Should().BeOnOrAfter(modifiedBefore);
    }
}
