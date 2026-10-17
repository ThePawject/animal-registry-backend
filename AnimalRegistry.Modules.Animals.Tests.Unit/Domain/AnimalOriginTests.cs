using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared.DDD;
using FluentAssertions;
using static AnimalRegistry.Modules.Animals.Tests.Unit.OriginTestData;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Domain;

public class AnimalOriginTests
{
    private readonly Municipality _found = CreateMunicipality("Gmina A");
    private readonly Municipality _contractor = CreateMunicipality("Gmina B");
    private readonly Guid _animalId = Guid.NewGuid();

    private AnimalOrigin Create(AnimalOriginData data, params Municipality[] municipalities)
    {
        return AnimalOrigin.Create(_animalId, ShelterId, data,
            municipalities.Length == 0 ? [_found, _contractor] : municipalities, StartOfNextDay);
    }

    [Fact]
    public void Create_ShouldStoreDataAndTrimStreet_WhenDataIsValid()
    {
        var origin = Create(Data(_found.Id, _contractor.Id, street: "  Leśna 1 "));

        origin.AnimalId.Should().Be(_animalId);
        origin.ShelterId.Should().Be(ShelterId);
        origin.AdmissionDate.Should().Be(AdmissionDate);
        origin.AdmissionMethod.Should().Be(AdmissionMethod.Abandoned);
        origin.FoundStreet.Should().Be("Leśna 1");
        origin.FoundMunicipalityId.Should().Be(_found.Id);
        origin.ContractorMunicipalityId.Should().Be(_contractor.Id);
    }

    [Fact]
    public void Create_ShouldAcceptAdmissionDate_BeforeStartOfNextDay()
    {
        var act = () => Create(Data(_found.Id, _contractor.Id, StartOfNextDay.AddTicks(-1)));

        act.Should().NotThrow();
    }

    [Fact]
    public void Create_ShouldThrow_WhenAdmissionDateIsInFuture()
    {
        var act = () => Create(Data(_found.Id, _contractor.Id, StartOfNextDay));

        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Admission date cannot be in the future.");
    }

    [Theory]
    [InlineData(AdmissionMethod.Unassigned)]
    [InlineData((AdmissionMethod)999)]
    public void Create_ShouldThrow_WhenAdmissionMethodIsNotAssigned(AdmissionMethod method)
    {
        var act = () => Create(Data(_found.Id, _contractor.Id, method: method));

        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Admission method must be provided.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenStreetIsMissing(string street)
    {
        var act = () => Create(Data(_found.Id, _contractor.Id, street: street));

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Street where the animal was found must be provided.");
    }

    [Fact]
    public void Create_ShouldThrow_WhenMunicipalityIsMissing()
    {
        var withoutFound = () => Create(Data(Guid.Empty, _contractor.Id));
        var withoutContractor = () => Create(Data(_found.Id, Guid.Empty));

        withoutFound.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("The found municipality must be provided.");
        withoutContractor.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("The contractor municipality must be provided.");
    }

    [Fact]
    public void Create_ShouldThrow_WhenMunicipalityDoesNotExist()
    {
        var unknownId = Guid.NewGuid();

        var act = () => Create(Data(_found.Id, unknownId));

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage($"The contractor municipality '{unknownId}' does not exist.");
    }

    [Fact]
    public void Create_ShouldThrow_WhenMunicipalityBelongsToAnotherShelter()
    {
        var foreign = Municipality.Create("other-shelter", "Gmina Obca");

        var act = () => Create(Data(foreign.Id, _contractor.Id), foreign, _contractor);

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage($"The found municipality '{foreign.Id}' does not exist.");
    }

    [Fact]
    public void Create_ShouldThrow_WhenArchivedMunicipalityIsAssigned()
    {
        var archived = CreateMunicipality("Gmina Archiwalna", archived: true);

        var act = () => Create(Data(_found.Id, archived.Id), _found, archived);

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("The contractor municipality 'Gmina Archiwalna' is archived.");
    }

    [Fact]
    public void Update_ShouldReplaceData()
    {
        var origin = Create(Data(_found.Id, _found.Id));
        var date = AdmissionDate.AddDays(-3);

        origin.Update(Data(_contractor.Id, _contractor.Id, date, AdmissionMethod.Other, "Polna 2"),
            [_contractor], StartOfNextDay);

        origin.AdmissionDate.Should().Be(date);
        origin.AdmissionMethod.Should().Be(AdmissionMethod.Other);
        origin.FoundStreet.Should().Be("Polna 2");
        origin.FoundMunicipalityId.Should().Be(_contractor.Id);
        origin.ContractorMunicipalityId.Should().Be(_contractor.Id);
    }

    [Fact]
    public void Update_ShouldKeepData_WhenRuleIsBroken()
    {
        var origin = Create(Data(_found.Id, _contractor.Id));

        var act = () => origin.Update(Data(_found.Id, _contractor.Id, street: " "), [_found, _contractor],
            StartOfNextDay);

        act.Should().Throw<BusinessRuleValidationException>();
        origin.FoundStreet.Should().Be("Leśna 1");
    }

    [Fact]
    public void Update_ShouldAcceptArchivedMunicipality_WhenAlreadyAssignedInSameRole()
    {
        var origin = Create(Data(_found.Id, _contractor.Id));
        _contractor.Archive();

        var act = () => origin.Update(Data(_found.Id, _contractor.Id, street: "Polna 2"), [_found, _contractor],
            StartOfNextDay);

        act.Should().NotThrow();
        origin.FoundStreet.Should().Be("Polna 2");
    }

    [Fact]
    public void Update_ShouldAcceptArchivedMunicipality_WhenAlreadyAssignedInOtherRole()
    {
        var origin = Create(Data(_found.Id, _contractor.Id));
        _found.Archive();

        var act = () => origin.Update(Data(_found.Id, _found.Id), [_found, _contractor], StartOfNextDay);

        act.Should().NotThrow();
        origin.ContractorMunicipalityId.Should().Be(_found.Id);
    }

    [Fact]
    public void Update_ShouldThrow_WhenArchivedMunicipalityWasNotAssigned()
    {
        var origin = Create(Data(_found.Id, _found.Id));
        _contractor.Archive();

        var act = () => origin.Update(Data(_found.Id, _contractor.Id), [_found, _contractor], StartOfNextDay);

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("The contractor municipality 'Gmina B' is archived.");
    }
}
