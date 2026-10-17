using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Application.AnimalHealth;
using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using FluentAssertions;
using NSubstitute;
using static AnimalRegistry.Modules.Animals.Tests.Unit.VaccinationTestData;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application.Vaccinations;

public sealed class SetVaccinationAttachmentCommandHandlerTests
{
    private const string NewBlobPath = "test-shelter-id/animal/new.pdf";
    private const string OldBlobPath = "test-shelter-id/animal/old.pdf";

    private readonly IAnimalVaccinationRepository _vaccinations = Substitute.For<IAnimalVaccinationRepository>();
    private readonly IAnimalsUnitOfWork _unitOfWork = Substitute.For<IAnimalsUnitOfWork>();
    private readonly IBlobStorageService _blobs = Substitute.For<IBlobStorageService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly SetVaccinationAttachmentCommandHandler _handler;
    private readonly AnimalVaccination _vaccination = CreateVaccination();

    public SetVaccinationAttachmentCommandHandlerTests()
    {
        _currentUser.ShelterId.Returns(ShelterId);
        _vaccinations.GetByIdAsync(_vaccination.Id, _vaccination.AnimalId, ShelterId, Arg.Any<CancellationToken>())
            .Returns(_vaccination);
        _blobs.UploadDocumentAsync(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), ShelterId,
                _vaccination.AnimalId, Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success(NewBlobPath));
        _blobs.GetBlobUrl(Arg.Any<string>()).Returns(call => $"https://blob/{call.Arg<string>()}");
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success());
        _handler = new SetVaccinationAttachmentCommandHandler(_vaccinations, _unitOfWork, _blobs, _currentUser);
    }

    private SetVaccinationAttachmentCommand Command(Guid? vaccinationId = null)
    {
        return new SetVaccinationAttachmentCommand(_vaccination.AnimalId, vaccinationId ?? _vaccination.Id,
            new DocumentUploadInfo("scan.pdf", new MemoryStream([1, 2, 3]), "application/pdf"));
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenVaccinationDoesNotExist()
    {
        var result = await _handler.Handle(Command(Guid.NewGuid()), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
        await _blobs.DidNotReceiveWithAnyArgs().UploadDocumentAsync(default!, default!, default!, default!,
            default, default);
    }

    [Fact]
    public async Task Handle_ShouldStoreAttachment_WhenUploadSucceeds()
    {
        var result = await _handler.Handle(Command(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _vaccination.Attachment!.BlobPath.Should().Be(NewBlobPath);
        _vaccination.Attachment.FileName.Should().Be("scan.pdf");
        result.Value!.Attachment!.Url.Should().Be($"https://blob/{NewBlobPath}");
        await _blobs.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldDeletePreviousBlob_AfterSaving()
    {
        _vaccination.SetAttachment(VaccinationAttachment.Create(OldBlobPath, "old.pdf", "application/pdf", Now), Now);

        await _handler.Handle(Command(), CancellationToken.None);

        Received.InOrder(() =>
        {
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
            _blobs.DeleteAsync(OldBlobPath, Arg.Any<CancellationToken>());
        });
        await _blobs.DidNotReceive().DeleteAsync(NewBlobPath, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldDeleteNewBlobAndKeepOld_WhenSaveFails()
    {
        _vaccination.SetAttachment(VaccinationAttachment.Create(OldBlobPath, "old.pdf", "application/pdf", Now), Now);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.ValidationError("conflict"));

        var result = await _handler.Handle(Command(), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.ValidationError);
        await _blobs.Received(1).DeleteAsync(NewBlobPath, Arg.Any<CancellationToken>());
        await _blobs.DidNotReceive().DeleteAsync(OldBlobPath, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldDeleteNewBlobAndRethrow_WhenSaveThrows()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns<Result>(_ => throw new InvalidOperationException("db down"));

        var act = () => _handler.Handle(Command(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _blobs.Received(1).DeleteAsync(NewBlobPath, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenUploadFails()
    {
        _blobs.UploadDocumentAsync(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), ShelterId,
                _vaccination.AnimalId, Arg.Any<CancellationToken>())
            .Returns(Result<string>.ValidationError("bad file"));

        var result = await _handler.Handle(Command(), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.ValidationError);
        _vaccination.Attachment.Should().BeNull();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
