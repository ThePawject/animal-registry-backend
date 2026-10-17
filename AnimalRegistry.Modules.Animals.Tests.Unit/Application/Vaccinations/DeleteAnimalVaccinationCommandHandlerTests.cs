using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using FluentAssertions;
using NSubstitute;
using static AnimalRegistry.Modules.Animals.Tests.Unit.VaccinationTestData;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application.Vaccinations;

public sealed class DeleteAnimalVaccinationCommandHandlerTests
{
    private readonly IAnimalVaccinationRepository _vaccinations = Substitute.For<IAnimalVaccinationRepository>();
    private readonly IAnimalsUnitOfWork _unitOfWork = Substitute.For<IAnimalsUnitOfWork>();
    private readonly IBlobStorageService _blobs = Substitute.For<IBlobStorageService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly DeleteAnimalVaccinationCommandHandler _handler;
    private readonly AnimalVaccination _vaccination = CreateVaccination();

    public DeleteAnimalVaccinationCommandHandlerTests()
    {
        _currentUser.ShelterId.Returns(ShelterId);
        _vaccinations.GetByIdAsync(_vaccination.Id, _vaccination.AnimalId, ShelterId, Arg.Any<CancellationToken>())
            .Returns(_vaccination);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success());
        _handler = new DeleteAnimalVaccinationCommandHandler(_vaccinations, _unitOfWork, _blobs, _currentUser);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenVaccinationBelongsToAnotherAnimal()
    {
        var result = await _handler.Handle(new DeleteAnimalVaccinationCommand(Guid.NewGuid(), _vaccination.Id),
            CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
        _vaccinations.DidNotReceiveWithAnyArgs().Remove(default!);
    }

    [Fact]
    public async Task Handle_ShouldRemoveVaccinationAndAttachmentBlob()
    {
        _vaccination.SetAttachment(VaccinationAttachment.Create("path.pdf", "scan.pdf", "application/pdf", Now), Now);

        var result = await _handler.Handle(new DeleteAnimalVaccinationCommand(_vaccination.AnimalId, _vaccination.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _vaccinations.Received(1).Remove(_vaccination);
        Received.InOrder(() =>
        {
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
            _blobs.DeleteAsync("path.pdf", Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_ShouldKeepBlob_WhenSaveFails()
    {
        _vaccination.SetAttachment(VaccinationAttachment.Create("path.pdf", "scan.pdf", "application/pdf", Now), Now);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.ValidationError("conflict"));

        var result = await _handler.Handle(new DeleteAnimalVaccinationCommand(_vaccination.AnimalId, _vaccination.Id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await _blobs.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
