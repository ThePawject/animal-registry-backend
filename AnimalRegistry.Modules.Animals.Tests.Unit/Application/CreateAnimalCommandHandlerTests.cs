using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using FluentAssertions;
using NSubstitute;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application;

public class CreateAnimalCommandHandlerTests
{
    private const string TestShelterId = "test-shelter-id";

    private static ICurrentUser CreateCurrentUserMock()
    {
        var currentUserMock = Substitute.For<ICurrentUser>();
        currentUserMock.ShelterId.Returns(TestShelterId);
        return currentUserMock;
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
            AnimalSpecies.Dog,
            AnimalSex.Male,
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
        );
        var repoMock = Substitute.For<IAnimalRepository>();
        repoMock.AddAsync(Arg.Any<Animal>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(Result<Animal>.Success(ci.ArgAt<Animal>(0))));
        var currentUserMock = CreateCurrentUserMock();
        var blobStorageMock = Substitute.For<IBlobStorageService>();
        var signatureServiceMock = Substitute.For<IAnimalSignatureService>();
        signatureServiceMock.IsSignatureUniqueAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new CreateAnimalCommandHandler(repoMock, signatureServiceMock, currentUserMock, blobStorageMock);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();
        response.Value.Should().BeOfType<CreateAnimalCommandResponse>();
    }

    [Fact]
    public async Task Handle_ShouldWork_WithDifferentSpeciesAndSex()
    {
        var command = new CreateAnimalCommand(
            Sig("2024/0002"),
            "TR999",
            "Mruczek",
            "Gray",
            AnimalSpecies.Cat,
            AnimalSex.Female,
            new DateTimeOffset(2018, 5, 10, 0, 0, 0, TimeSpan.Zero)
        );
        var repoMock = Substitute.For<IAnimalRepository>();
        repoMock.AddAsync(Arg.Any<Animal>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(Result<Animal>.Success(ci.ArgAt<Animal>(0))));
        var currentUserMock = CreateCurrentUserMock();
        var blobStorageMock = Substitute.For<IBlobStorageService>();
        var signatureServiceMock = Substitute.For<IAnimalSignatureService>();
        signatureServiceMock.IsSignatureUniqueAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new CreateAnimalCommandHandler(repoMock, signatureServiceMock, currentUserMock, blobStorageMock);

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
            AnimalSpecies.Dog,
            AnimalSex.Male,
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
        );
        var repoMock = Substitute.For<IAnimalRepository>();
        var currentUserMock = CreateCurrentUserMock();
        var blobStorageMock = Substitute.For<IBlobStorageService>();
        var signatureServiceMock = Substitute.For<IAnimalSignatureService>();
        signatureServiceMock.IsSignatureUniqueAsync("2024/0003", TestShelterId, null, Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new CreateAnimalCommandHandler(repoMock, signatureServiceMock, currentUserMock, blobStorageMock);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Status.Should().Be(ResultStatus.ValidationError);
        response.Error.Should().Contain("2024/0003");
    }
}