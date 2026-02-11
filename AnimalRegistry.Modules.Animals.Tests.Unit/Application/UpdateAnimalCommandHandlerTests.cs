using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using FluentAssertions;
using NSubstitute;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application;

public class UpdateAnimalCommandHandlerTests
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
    public async Task Handle_ShouldUpdateAnimal_WhenRequestIsValid()
    {
        var existingAnimal = Animal.Create(
            Sig("2024/0001"),
            "TR123",
            "Burek",
            "Brown",
            AnimalSpecies.Dog,
            AnimalSex.Male,
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            TestShelterId
        );
        var command = new UpdateAnimalCommand(
            existingAnimal.Id,
            Sig("2024/0002"),
            "TR456",
            "Reksio",
            "Black",
            AnimalSpecies.Dog,
            AnimalSex.Male,
            new DateTimeOffset(2019, 6, 15, 0, 0, 0, TimeSpan.Zero),
            [],
            [],
            null,
            null
        );
        var repoMock = Substitute.For<IAnimalRepository>();
        repoMock.GetByIdAsync(existingAnimal.Id, TestShelterId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Animal?>(existingAnimal));
        repoMock.UpdateAsync(Arg.Any<Animal>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(Result<Animal>.Success(ci.ArgAt<Animal>(0))));
        var currentUserMock = CreateCurrentUserMock();
        var blobStorageMock = Substitute.For<IBlobStorageService>();
        var signatureServiceMock = Substitute.For<IAnimalSignatureService>();
        signatureServiceMock
            .IsSignatureUniqueAsync("2024/0002", TestShelterId, existingAnimal.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new UpdateAnimalCommandHandler(repoMock, signatureServiceMock, currentUserMock, blobStorageMock);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();
        response.Value.Should().BeOfType<UpdateAnimalCommandResponse>();
        response.Value!.AnimalId.Should().Be(existingAnimal.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAnimalDoesNotExist()
    {
        var command = new UpdateAnimalCommand(
            Guid.NewGuid(),
            Sig("2024/0003"),
            "TR456",
            "Reksio",
            "Black",
            AnimalSpecies.Dog,
            AnimalSex.Male,
            new DateTimeOffset(2019, 6, 15, 0, 0, 0, TimeSpan.Zero),
            [],
            [],
            null,
            null
        );
        var repoMock = Substitute.For<IAnimalRepository>();
        repoMock.GetByIdAsync(Arg.Any<Guid>(), TestShelterId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Animal?>(null));
        var currentUserMock = CreateCurrentUserMock();
        var blobStorageMock = Substitute.For<IBlobStorageService>();
        var signatureServiceMock = Substitute.For<IAnimalSignatureService>();

        var handler = new UpdateAnimalCommandHandler(repoMock, signatureServiceMock, currentUserMock, blobStorageMock);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldWork_WithDifferentSpeciesAndSex()
    {
        var existingAnimal = Animal.Create(
            Sig("2024/0004"),
            "TR999",
            "Mruczek",
            "Gray",
            AnimalSpecies.Cat,
            AnimalSex.Female,
            new DateTimeOffset(2018, 5, 10, 0, 0, 0, TimeSpan.Zero),
            TestShelterId
        );
        var command = new UpdateAnimalCommand(
            existingAnimal.Id,
            Sig("2024/0005"),
            "TR888",
            "Kitty",
            "White",
            AnimalSpecies.Cat,
            AnimalSex.Female,
            new DateTimeOffset(2017, 3, 20, 0, 0, 0, TimeSpan.Zero),
            [],
            [],
            null,
            null
        );
        var repoMock = Substitute.For<IAnimalRepository>();
        repoMock.GetByIdAsync(existingAnimal.Id, TestShelterId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Animal?>(existingAnimal));
        repoMock.UpdateAsync(Arg.Any<Animal>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(Result<Animal>.Success(ci.ArgAt<Animal>(0))));
        var currentUserMock = CreateCurrentUserMock();
        var blobStorageMock = Substitute.For<IBlobStorageService>();
        var signatureServiceMock = Substitute.For<IAnimalSignatureService>();
        signatureServiceMock
            .IsSignatureUniqueAsync("2024/0005", TestShelterId, existingAnimal.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new UpdateAnimalCommandHandler(repoMock, signatureServiceMock, currentUserMock, blobStorageMock);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();
        response.Value.Should().BeOfType<UpdateAnimalCommandResponse>();
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenSignatureNotUnique()
    {
        var existingAnimal = Animal.Create(
            Sig("2024/0006"),
            "TR123",
            "Burek",
            "Brown",
            AnimalSpecies.Dog,
            AnimalSex.Male,
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            TestShelterId
        );
        var command = new UpdateAnimalCommand(
            existingAnimal.Id,
            Sig("2024/0007"),
            "TR456",
            "Reksio",
            "Black",
            AnimalSpecies.Dog,
            AnimalSex.Male,
            new DateTimeOffset(2019, 6, 15, 0, 0, 0, TimeSpan.Zero),
            [],
            [],
            null,
            null
        );
        var repoMock = Substitute.For<IAnimalRepository>();
        repoMock.GetByIdAsync(existingAnimal.Id, TestShelterId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Animal?>(existingAnimal));
        var currentUserMock = CreateCurrentUserMock();
        var blobStorageMock = Substitute.For<IBlobStorageService>();
        var signatureServiceMock = Substitute.For<IAnimalSignatureService>();
        signatureServiceMock
            .IsSignatureUniqueAsync("2024/0007", TestShelterId, existingAnimal.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new UpdateAnimalCommandHandler(repoMock, signatureServiceMock, currentUserMock, blobStorageMock);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Status.Should().Be(ResultStatus.ValidationError);
        response.Error.Should().Contain("2024/0007");
    }

    [Fact]
    public async Task Handle_ShouldAllowUpdate_WhenSignatureUnchanged()
    {
        var existingAnimal = Animal.Create(
            Sig("2024/0008"),
            "TR123",
            "Burek",
            "Brown",
            AnimalSpecies.Dog,
            AnimalSex.Male,
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            TestShelterId
        );
        var command = new UpdateAnimalCommand(
            existingAnimal.Id,
            Sig("2024/0008"),
            "TR456",
            "Reksio",
            "Black",
            AnimalSpecies.Dog,
            AnimalSex.Male,
            new DateTimeOffset(2019, 6, 15, 0, 0, 0, TimeSpan.Zero),
            [],
            [],
            null,
            null
        );
        var repoMock = Substitute.For<IAnimalRepository>();
        repoMock.GetByIdAsync(existingAnimal.Id, TestShelterId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Animal?>(existingAnimal));
        repoMock.UpdateAsync(Arg.Any<Animal>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(Result<Animal>.Success(ci.ArgAt<Animal>(0))));
        var currentUserMock = CreateCurrentUserMock();
        var blobStorageMock = Substitute.For<IBlobStorageService>();
        var signatureServiceMock = Substitute.For<IAnimalSignatureService>();

        var handler = new UpdateAnimalCommandHandler(repoMock, signatureServiceMock, currentUserMock, blobStorageMock);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}