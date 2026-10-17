using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using FluentAssertions;
using NSubstitute;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application;

public class CreateAnimalCommandHandlerTests
{
    private readonly IAnimalsUnitOfWork _unitOfWork = Substitute.For<IAnimalsUnitOfWork>();

    public CreateAnimalCommandHandlerTests()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success());
    }

    private const string TestShelterId = "test-shelter-id";

    private static ICurrentUser CreateCurrentUserMock()
    {
        var currentUserMock = Substitute.For<ICurrentUser>();
        currentUserMock.ShelterId.Returns(TestShelterId);
        return currentUserMock;
    }

    private static IAnimalOriginWriter CreateOriginWriterMock(Result? result = null)
    {
        var originWriterMock = Substitute.For<IAnimalOriginWriter>();
        originWriterMock.ApplyAsync(Arg.Any<Animal>(), Arg.Any<AnimalOriginData?>(), Arg.Any<CancellationToken>())
            .Returns(result ?? Result.Success());
        return originWriterMock;
    }

    private static AnimalSignature Sig(string signature)
    {
        return AnimalSignature.Create(signature).Value!;
    }

    [Fact]
    public async Task Handle_ShouldCreateAnimal_WhenRequestIsValid()
    {
        var command = new CreateAnimalCommand(
            Sig("2024/0001"),
            "TR123",
            "Burek",
            "Brown",
            "labrador",
            "tail",
            AnimalSpecies.Dog,
            AnimalSex.Male,
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
        );
        var repoMock = Substitute.For<IAnimalRepository>();
        var currentUserMock = CreateCurrentUserMock();
        var blobStorageMock = Substitute.For<IBlobStorageService>();
        var signatureServiceMock = Substitute.For<IAnimalSignatureService>();
        signatureServiceMock.IsSignatureUniqueAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AnimalSpecies>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new CreateAnimalCommandHandler(repoMock, signatureServiceMock, currentUserMock, blobStorageMock,
            CreateOriginWriterMock(), _unitOfWork);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        repoMock.Received(1).Add(Arg.Any<Animal>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        response.Value.Should().NotBeNull();
        response.Value.Should().BeOfType<CreateAnimalCommandResponse>();
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenSavingFails()
    {
        var command = new CreateAnimalCommand(
            Sig("2024/0003"),
            "TR124",
            "Burek",
            "Brown",
            "labrador",
            "tail",
            AnimalSpecies.Dog,
            AnimalSex.Male,
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
        );
        var signatureServiceMock = Substitute.For<IAnimalSignatureService>();
        signatureServiceMock.IsSignatureUniqueAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AnimalSpecies>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .Returns(true);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.ValidationError("Conflict."));

        var handler = new CreateAnimalCommandHandler(Substitute.For<IAnimalRepository>(), signatureServiceMock,
            CreateCurrentUserMock(), Substitute.For<IBlobStorageService>(), CreateOriginWriterMock(), _unitOfWork);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Status.Should().Be(ResultStatus.ValidationError);
        response.Error.Should().Be("Conflict.");
    }

    [Fact]
    public async Task Handle_ShouldWork_WithDifferentSpeciesAndSex()
    {
        var command = new CreateAnimalCommand(
            Sig("2024/0002"),
            "TR999",
            "Mruczek",
            "Gray",
            "labrador",
            "tail",
            AnimalSpecies.Cat,
            AnimalSex.Female,
            new DateTimeOffset(2018, 5, 10, 0, 0, 0, TimeSpan.Zero)
        );
        var repoMock = Substitute.For<IAnimalRepository>();
        var currentUserMock = CreateCurrentUserMock();
        var blobStorageMock = Substitute.For<IBlobStorageService>();
        var signatureServiceMock = Substitute.For<IAnimalSignatureService>();
        signatureServiceMock.IsSignatureUniqueAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AnimalSpecies>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new CreateAnimalCommandHandler(repoMock, signatureServiceMock, currentUserMock, blobStorageMock,
            CreateOriginWriterMock(), _unitOfWork);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();
        response.Value.Should().BeOfType<CreateAnimalCommandResponse>();
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenSignatureNotUnique()
    {
        var command = new CreateAnimalCommand(
            Sig("2024/0003"),
            "TR123",
            "Burek",
            "Brown",
            "labrador",
            "tail",
            AnimalSpecies.Dog,
            AnimalSex.Male,
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
        );
        var repoMock = Substitute.For<IAnimalRepository>();
        var currentUserMock = CreateCurrentUserMock();
        var blobStorageMock = Substitute.For<IBlobStorageService>();
        var signatureServiceMock = Substitute.For<IAnimalSignatureService>();
        signatureServiceMock.IsSignatureUniqueAsync("2024/0003", TestShelterId, AnimalSpecies.Dog, null,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new CreateAnimalCommandHandler(repoMock, signatureServiceMock, currentUserMock, blobStorageMock,
            CreateOriginWriterMock(), _unitOfWork);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Status.Should().Be(ResultStatus.ValidationError);
        response.Error.Should().Contain("2024/0003");
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationErrorAndNotSave_WhenOriginIsRejected()
    {
        var command = new CreateAnimalCommand(
            Sig("2024/0004"),
            "TR123",
            "Burek",
            "Brown",
            "labrador",
            "tail",
            AnimalSpecies.Dog,
            AnimalSex.Male,
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
        );
        var repoMock = Substitute.For<IAnimalRepository>();
        var currentUserMock = CreateCurrentUserMock();
        var blobStorageMock = Substitute.For<IBlobStorageService>();
        var signatureServiceMock = Substitute.For<IAnimalSignatureService>();
        signatureServiceMock.IsSignatureUniqueAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AnimalSpecies>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new CreateAnimalCommandHandler(repoMock, signatureServiceMock, currentUserMock, blobStorageMock,
            CreateOriginWriterMock(Result.ValidationError("Animal origin is required.")), _unitOfWork);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Status.Should().Be(ResultStatus.ValidationError);
        response.Error.Should().Be("Animal origin is required.");
        repoMock.DidNotReceiveWithAnyArgs().Add(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync();
    }
}
