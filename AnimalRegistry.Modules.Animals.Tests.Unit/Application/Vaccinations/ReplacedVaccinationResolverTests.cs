using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using FluentAssertions;
using NSubstitute;
using static AnimalRegistry.Modules.Animals.Tests.Unit.VaccinationTestData;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application.Vaccinations;

public sealed class ReplacedVaccinationResolverTests
{
    private readonly IAnimalVaccinationRepository _vaccinations = Substitute.For<IAnimalVaccinationRepository>();
    private readonly ReplacedVaccinationResolver _resolver;
    private readonly AnimalVaccination _replaced = CreateVaccination();

    public ReplacedVaccinationResolverTests()
    {
        _vaccinations.GetByIdAsync(_replaced.Id, _replaced.AnimalId, ShelterId, Arg.Any<CancellationToken>())
            .Returns(_replaced);
        _resolver = new ReplacedVaccinationResolver(_vaccinations);
    }

    private Task<Result<ReplacedVaccination>> ResolveAsync(Guid? replacesId, Guid? replacingId = null)
    {
        return _resolver.ResolveAsync(replacesId, _replaced.AnimalId, replacingId, ShelterId, CancellationToken.None);
    }

    [Fact]
    public async Task ResolveAsync_ShouldReturnNone_WhenNothingIsReplaced()
    {
        var result = await ResolveAsync(null);

        result.Value.Should().Be(ReplacedVaccination.None);
        await _vaccinations.DidNotReceiveWithAnyArgs().GetByIdAsync(default, default, default!, default);
    }

    [Fact]
    public async Task ResolveAsync_ShouldReturnValidationError_WhenReplacedVaccinationIsNotFound()
    {
        var result = await ResolveAsync(Guid.NewGuid());

        result.Status.Should().Be(ResultStatus.ValidationError);
        result.Error.Should().Be("Replaced vaccination not found.");
    }

    [Fact]
    public async Task ResolveAsync_ShouldReportReplacedElsewhere_WhenAnotherVaccinationReplacesIt()
    {
        var other = CreateVaccination(_replaced.AnimalId);
        _vaccinations.FindReplacingAsync(_replaced.Id, ShelterId, Arg.Any<CancellationToken>()).Returns(other);

        var result = await ResolveAsync(_replaced.Id, Guid.NewGuid());

        result.Value!.Vaccination.Should().BeSameAs(_replaced);
        result.Value.IsReplacedElsewhere.Should().BeTrue();
    }

    [Fact]
    public async Task ResolveAsync_ShouldNotReportReplacedElsewhere_WhenCurrentVaccinationAlreadyReplacesIt()
    {
        var current = CreateVaccination(_replaced.AnimalId);
        _vaccinations.FindReplacingAsync(_replaced.Id, ShelterId, Arg.Any<CancellationToken>()).Returns(current);

        var result = await ResolveAsync(_replaced.Id, current.Id);

        result.Value!.IsReplacedElsewhere.Should().BeFalse();
    }
}
