using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.DDD;
using AnimalRegistry.Shared.ShelterFeatures;
using FluentAssertions;
using NSubstitute;
using static AnimalRegistry.Modules.Animals.Tests.Unit.OriginTestData;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application.Origin;

public class AnimalOriginWriterTests
{
    private readonly IShelterFeatureToggles _toggles = Substitute.For<IShelterFeatureToggles>();
    private readonly IAnimalOriginRepository _origins = Substitute.For<IAnimalOriginRepository>();
    private readonly IMunicipalityRepository _municipalities = Substitute.For<IMunicipalityRepository>();
    private readonly AnimalOriginWriter _writer;

    public AnimalOriginWriterTests()
    {
        _writer = new AnimalOriginWriter(_toggles, _origins, _municipalities);
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

    [Fact]
    public async Task ApplyAsync_ShouldIgnoreData_WhenModuleIsDisabled()
    {
        EnableModule(false);

        var result = await _writer.ApplyAsync(CreateAnimal(), Data(Guid.NewGuid(), Guid.NewGuid()));

        result.IsSuccess.Should().BeTrue();
        await _origins.DidNotReceiveWithAnyArgs().GetByAnimalIdAsync(default, default!);
        _origins.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task ApplyAsync_ShouldReturnValidationError_WhenModuleIsEnabledAndDataIsMissing()
    {
        EnableModule();

        var result = await _writer.ApplyAsync(CreateAnimal(), null);

        result.Status.Should().Be(ResultStatus.ValidationError);
        result.Error.Should().Be("Animal origin is required when the origin module is enabled.");
    }

    [Fact]
    public async Task ApplyAsync_ShouldAddNewOrigin_WhenAnimalHasNone()
    {
        EnableModule();
        var found = CreateMunicipality("Gmina A");
        var contractor = CreateMunicipality("Gmina B");
        GivenMunicipalities(found, contractor);
        var animal = CreateAnimal();

        var result = await _writer.ApplyAsync(animal, Data(found.Id, contractor.Id));

        result.IsSuccess.Should().BeTrue();
        _origins.Received(1).Add(Arg.Is<AnimalOrigin>(o =>
            o.AnimalId == animal.Id &&
            o.ShelterId == ShelterId &&
            o.FoundMunicipalityId == found.Id &&
            o.ContractorMunicipalityId == contractor.Id));
    }

    [Fact]
    public async Task ApplyAsync_ShouldUpdateExistingOrigin()
    {
        EnableModule();
        var municipality = CreateMunicipality("Gmina A");
        GivenMunicipalities(municipality);
        var animal = CreateAnimal();
        var origin = CreateOrigin(animal, municipality, municipality);
        _origins.GetByAnimalIdAsync(animal.Id, ShelterId, Arg.Any<CancellationToken>()).Returns(origin);

        var result = await _writer.ApplyAsync(animal,
            Data(municipality.Id, municipality.Id, method: AdmissionMethod.Other, street: "Polna 2"));

        result.IsSuccess.Should().BeTrue();
        origin.FoundStreet.Should().Be("Polna 2");
        origin.AdmissionMethod.Should().Be(AdmissionMethod.Other);
        _origins.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task ApplyAsync_ShouldThrow_WhenDomainRuleIsBroken()
    {
        EnableModule();
        var found = CreateMunicipality("Gmina A");
        GivenMunicipalities(found);
        var unknownId = Guid.NewGuid();

        var act = () => _writer.ApplyAsync(CreateAnimal(), Data(found.Id, unknownId));

        await act.Should().ThrowAsync<BusinessRuleValidationException>()
            .WithMessage($"The contractor municipality '{unknownId}' does not exist.");
        _origins.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task ApplyAsync_ShouldThrow_WhenAdmissionDateIsAfterToday()
    {
        EnableModule();
        var municipality = CreateMunicipality("Gmina A");
        GivenMunicipalities(municipality);

        var act = () => _writer.ApplyAsync(CreateAnimal(),
            Data(municipality.Id, municipality.Id, DateTimeOffset.UtcNow.AddDays(2)));

        await act.Should().ThrowAsync<BusinessRuleValidationException>()
            .WithMessage("Admission date cannot be in the future.");
    }
}
