using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared.DDD;
using FluentAssertions;
using static AnimalRegistry.Modules.Animals.Tests.Unit.OriginTestData;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Domain;

public class AnimalOriginTests
{
    private readonly Animal _animal = CreateAnimal();
    private readonly Municipality _found = CreateMunicipality("Gmina A");
    private readonly Municipality _contractor = CreateMunicipality("Gmina B");

    private void SetOrigin(AnimalOriginData data, params Municipality[] municipalities)
    {
        _animal.SetOrigin(AnimalOrigin.Create(data, Now),
            municipalities.Length == 0 ? [_found, _contractor] : municipalities);
    }

    [Fact]
    public void Create_ShouldStoreDataAndTrimStreet_WhenDataIsValid()
    {
        var origin = AnimalOrigin.Create(Data(_found.Id, _contractor.Id, street: "  Leśna 1 "), Now);

        origin.AdmissionDate.Should().Be(AdmissionDate);
        origin.AdmissionMethod.Should().Be(AdmissionMethod.Abandoned);
        origin.FoundStreet.Should().Be("Leśna 1");
        origin.FoundMunicipalityId.Should().Be(_found.Id);
        origin.ContractorMunicipalityId.Should().Be(_contractor.Id);
    }

    [Fact]
    public void Create_ShouldAcceptAdmissionDate_BeforeStartOfNextDay()
    {
        var act = () => AnimalOrigin.Create(Data(_found.Id, _contractor.Id, StartOfNextDay.AddTicks(-1)), Now);

        act.Should().NotThrow();
    }

    [Fact]
    public void Create_ShouldThrow_WhenAdmissionDateIsInFuture()
    {
        var act = () => AnimalOrigin.Create(Data(_found.Id, _contractor.Id, StartOfNextDay), Now);

        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Admission date cannot be in the future.");
    }

    [Theory]
    [InlineData(AdmissionMethod.Unassigned)]
    [InlineData((AdmissionMethod)999)]
    public void Create_ShouldThrow_WhenAdmissionMethodIsNotAssigned(AdmissionMethod method)
    {
        var act = () => AnimalOrigin.Create(Data(_found.Id, _contractor.Id, method: method), Now);

        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Admission method must be provided.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenStreetIsMissing(string street)
    {
        var act = () => AnimalOrigin.Create(Data(_found.Id, _contractor.Id, street: street), Now);

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Street where the animal was found must be provided.");
    }

    [Fact]
    public void Create_ShouldThrow_WhenMunicipalityIsMissing()
    {
        var withoutFound = () => AnimalOrigin.Create(Data(Guid.Empty, _contractor.Id), Now);
        var withoutContractor = () => AnimalOrigin.Create(Data(_found.Id, Guid.Empty), Now);

        withoutFound.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("The found municipality must be provided.");
        withoutContractor.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("The contractor municipality must be provided.");
    }

    [Fact]
    public void Create_ShouldBeEqual_WhenDataIsEqual()
    {
        var data = Data(_found.Id, _contractor.Id);

        AnimalOrigin.Create(data, Now).Should().Be(AnimalOrigin.Create(data, Now));
    }

    [Fact]
    public void SetOrigin_ShouldAssignOrigin()
    {
        SetOrigin(Data(_found.Id, _contractor.Id));

        _animal.Origin!.FoundMunicipalityId.Should().Be(_found.Id);
        _animal.Origin.ContractorMunicipalityId.Should().Be(_contractor.Id);
    }

    [Fact]
    public void SetOrigin_ShouldThrow_WhenMunicipalityDoesNotExist()
    {
        var unknownId = Guid.NewGuid();

        var act = () => SetOrigin(Data(_found.Id, unknownId));

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage($"The contractor municipality '{unknownId}' does not exist.");
    }

    [Fact]
    public void SetOrigin_ShouldThrow_WhenMunicipalityBelongsToAnotherShelter()
    {
        var foreign = Municipality.Create("other-shelter", "Gmina Obca");

        var act = () => SetOrigin(Data(foreign.Id, _contractor.Id), foreign, _contractor);

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage($"The found municipality '{foreign.Id}' does not exist.");
    }

    [Fact]
    public void SetOrigin_ShouldThrow_WhenArchivedMunicipalityIsAssigned()
    {
        var archived = CreateMunicipality("Gmina Archiwalna", archived: true);

        var act = () => SetOrigin(Data(_found.Id, archived.Id), _found, archived);

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("The contractor municipality 'Gmina Archiwalna' is archived.");
    }

    [Fact]
    public void SetOrigin_ShouldReplaceExistingOrigin()
    {
        SetOrigin(Data(_found.Id, _found.Id));
        var date = AdmissionDate.AddDays(-3);

        SetOrigin(Data(_contractor.Id, _contractor.Id, date, AdmissionMethod.Other, "Polna 2"), _contractor);

        _animal.Origin!.AdmissionDate.Should().Be(date);
        _animal.Origin.AdmissionMethod.Should().Be(AdmissionMethod.Other);
        _animal.Origin.FoundStreet.Should().Be("Polna 2");
        _animal.Origin.FoundMunicipalityId.Should().Be(_contractor.Id);
        _animal.Origin.ContractorMunicipalityId.Should().Be(_contractor.Id);
    }

    [Fact]
    public void SetOrigin_ShouldKeepOrigin_WhenRuleIsBroken()
    {
        SetOrigin(Data(_found.Id, _contractor.Id));

        var act = () => SetOrigin(Data(_found.Id, Guid.NewGuid(), street: "Polna 2"));

        act.Should().Throw<BusinessRuleValidationException>();
        _animal.Origin!.FoundStreet.Should().Be("Leśna 1");
    }

    [Fact]
    public void SetOrigin_ShouldAcceptArchivedMunicipality_WhenAlreadyAssignedInSameRole()
    {
        SetOrigin(Data(_found.Id, _contractor.Id));
        _contractor.Archive();

        var act = () => SetOrigin(Data(_found.Id, _contractor.Id, street: "Polna 2"));

        act.Should().NotThrow();
        _animal.Origin!.FoundStreet.Should().Be("Polna 2");
    }

    [Fact]
    public void SetOrigin_ShouldAcceptArchivedMunicipality_WhenAlreadyAssignedInOtherRole()
    {
        SetOrigin(Data(_found.Id, _contractor.Id));
        _found.Archive();

        var act = () => SetOrigin(Data(_found.Id, _found.Id));

        act.Should().NotThrow();
        _animal.Origin!.ContractorMunicipalityId.Should().Be(_found.Id);
    }

    [Fact]
    public void SetOrigin_ShouldThrow_WhenArchivedMunicipalityWasNotAssigned()
    {
        SetOrigin(Data(_found.Id, _found.Id));
        _contractor.Archive();

        var act = () => SetOrigin(Data(_found.Id, _contractor.Id));

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("The contractor municipality 'Gmina B' is archived.");
    }
}
