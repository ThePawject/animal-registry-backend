using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.ShelterFeatures;
using FluentAssertions;
using NSubstitute;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application.Origin;

public class AnimalOriginWriterTests
{
    private const string ShelterId = "test-shelter-id";
    private static readonly DateTimeOffset AdmissionDate = new(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);

    private readonly IShelterFeatureToggles _toggles = Substitute.For<IShelterFeatureToggles>();
    private readonly IMunicipalityRepository _municipalities = Substitute.For<IMunicipalityRepository>();
    private readonly AnimalOriginWriter _writer;

    public AnimalOriginWriterTests()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.ShelterId.Returns(ShelterId);
        _writer = new AnimalOriginWriter(_toggles, _municipalities, currentUser);
    }

    private static Animal CreateAnimal()
    {
        return Animal.Create(AnimalSignature.Create("2026/0001").Value!, null, "Burek", "Brown", "Mix", "",
            AnimalSpecies.Dog, AnimalSex.Male, null, ShelterId);
    }

    private void EnableModule(bool isEnabled = true)
    {
        _toggles.IsEnabledAsync(AnimalOriginFeature.FeatureKey, Arg.Any<CancellationToken>()).Returns(isEnabled);
    }

    private void GivenMunicipalities(params Municipality[] municipalities)
    {
        _municipalities.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), ShelterId, Arg.Any<CancellationToken>())
            .Returns(municipalities);
    }

    private static AnimalOriginInput Input(Guid foundId, Guid contractorId,
        AdmissionMethod method = AdmissionMethod.Abandoned, string street = "Leśna 1")
    {
        return new AnimalOriginInput(AdmissionDate, method, street, foundId, contractorId);
    }

    [Fact]
    public async Task ApplyAsync_ShouldIgnoreInputAndKeepOrigin_WhenModuleIsDisabled()
    {
        EnableModule(false);
        var animal = CreateAnimal();

        var result = await _writer.ApplyAsync(animal, Input(Guid.NewGuid(), Guid.NewGuid()));

        result.IsSuccess.Should().BeTrue();
        animal.Origin.Should().BeNull();
        await _municipalities.DidNotReceiveWithAnyArgs().GetByIdsAsync(default!, default!, default);
    }

    [Fact]
    public async Task ApplyAsync_ShouldReturnValidationError_WhenModuleIsEnabledAndInputIsMissing()
    {
        EnableModule();

        var result = await _writer.ApplyAsync(CreateAnimal(), null);

        result.Status.Should().Be(ResultStatus.ValidationError);
    }

    [Fact]
    public async Task ApplyAsync_ShouldReturnValidationError_WhenAdmissionMethodIsUnassigned()
    {
        EnableModule();
        var municipality = Municipality.Create(ShelterId, "Gmina");
        GivenMunicipalities(municipality);

        var result = await _writer.ApplyAsync(CreateAnimal(),
            Input(municipality.Id, municipality.Id, AdmissionMethod.Unassigned));

        result.Status.Should().Be(ResultStatus.ValidationError);
    }

    [Fact]
    public async Task ApplyAsync_ShouldSetOrigin_WhenMunicipalitiesExist()
    {
        EnableModule();
        var found = Municipality.Create(ShelterId, "Gmina A");
        var contractor = Municipality.Create(ShelterId, "Gmina B");
        GivenMunicipalities(found, contractor);
        var animal = CreateAnimal();

        var result = await _writer.ApplyAsync(animal, Input(found.Id, contractor.Id));

        result.IsSuccess.Should().BeTrue();
        animal.Origin.Should().NotBeNull();
        animal.Origin!.FoundMunicipalityId.Should().Be(found.Id);
        animal.Origin.ContractorMunicipalityId.Should().Be(contractor.Id);
    }

    [Fact]
    public async Task ApplyAsync_ShouldReturnValidationError_WhenMunicipalityDoesNotBelongToShelter()
    {
        EnableModule();
        var found = Municipality.Create(ShelterId, "Gmina A");
        GivenMunicipalities(found);

        var result = await _writer.ApplyAsync(CreateAnimal(), Input(found.Id, Guid.NewGuid()));

        result.Status.Should().Be(ResultStatus.ValidationError);
        result.Error.Should().Contain("contractor");
    }

    [Fact]
    public async Task ApplyAsync_ShouldRejectArchivedMunicipality_WhenNewlyAssigned()
    {
        EnableModule();
        var archived = Municipality.Create(ShelterId, "Gmina A");
        archived.Archive();
        GivenMunicipalities(archived);

        var result = await _writer.ApplyAsync(CreateAnimal(), Input(archived.Id, archived.Id));

        result.Status.Should().Be(ResultStatus.ValidationError);
        result.Error.Should().Contain("archived");
    }

    [Fact]
    public async Task ApplyAsync_ShouldAcceptArchivedMunicipality_WhenAnimalIsAlreadyAssignedToIt()
    {
        EnableModule();
        var municipality = Municipality.Create(ShelterId, "Gmina A");
        GivenMunicipalities(municipality);
        var animal = CreateAnimal();
        (await _writer.ApplyAsync(animal, Input(municipality.Id, municipality.Id))).IsSuccess.Should().BeTrue();
        municipality.Archive();

        var result = await _writer.ApplyAsync(animal, Input(municipality.Id, municipality.Id, street: "Polna 2"));

        result.IsSuccess.Should().BeTrue();
        animal.Origin!.FoundStreet.Should().Be("Polna 2");
    }
}
