using FluentAssertions;
using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using NSubstitute;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Application;

public class CreateAnimalCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateAnimal_WhenRequestIsValid()
    {
        var command = new CreateAnimalCommand(
            "SIG123",
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

        var handler = new CreateAnimalCommandHandler(repoMock);

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
            "SIG999",
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

        var handler = new CreateAnimalCommandHandler(repoMock);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();
        response.Value.Should().BeOfType<CreateAnimalCommandResponse>();
    }
}
