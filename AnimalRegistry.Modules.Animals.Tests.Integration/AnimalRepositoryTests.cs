using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Infrastructure;
using AnimalRegistry.Modules.Animals.Infrastructure.Animals;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace AnimalRegistry.Modules.Animals.Tests.Integration;

public sealed class AnimalRepositoryTests : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("yourStrong(!)Password")
        .Build();

    private AnimalsDbContext _dbContext = null!;
    private AnimalRepository _repository = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        var options = new DbContextOptionsBuilder<AnimalsDbContext>()
            .UseSqlServer(_dbContainer.GetConnectionString())
            .Options;
        _dbContext = new AnimalsDbContext(options);
        await _dbContext.Database.MigrateAsync();
        _repository = new AnimalRepository(_dbContext);
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    private const string TestShelterId = "test-shelter-id";

    [Fact]
    public async Task AddAndGetAnimal_WorksCorrectly()
    {
        var animal = Animal.Create(
            "sig1", "trans1", "Burek", "Brown", AnimalSpecies.Dog, AnimalSex.Male, DateTimeOffset.UtcNow.AddYears(-2), TestShelterId);
        await _repository.AddAsync(animal);
        var loaded = await _repository.GetByIdAsync(animal.Id, TestShelterId);
        Assert.NotNull(loaded);
        Assert.Equal("Burek", loaded!.Name);
        Assert.Equal(TestShelterId, loaded.ShelterId);
    }

    [Fact]
    public async Task RemoveAnimal_WorksCorrectly()
    {
        var animal = Animal.Create(
            "sig2", "trans2", "Mruczek", "Gray", AnimalSpecies.Cat, AnimalSex.Female,
            DateTimeOffset.UtcNow.AddYears(-3), TestShelterId);
        await _repository.AddAsync(animal);
        _repository.Remove(animal);
        var loaded = await _repository.GetByIdAsync(animal.Id, TestShelterId);
        Assert.Null(loaded);
    }

    [Fact]
    public async Task GetByIdAsync_WithWrongShelterId_ReturnsNull()
    {
        var animal = Animal.Create(
            "sig3", "trans3", "Reksio", "Black", AnimalSpecies.Dog, AnimalSex.Male, DateTimeOffset.UtcNow.AddYears(-1), TestShelterId);
        await _repository.AddAsync(animal);
        var loaded = await _repository.GetByIdAsync(animal.Id, "wrong-shelter-id");
        Assert.Null(loaded);
    }

    [Fact]
    public async Task ListAsync_WithShelterId_ReturnsOnlyMatchingAnimals()
    {
        var animal1 = Animal.Create(
            "sig4", "trans4", "Animal1", "Brown", AnimalSpecies.Dog, AnimalSex.Male, DateTimeOffset.UtcNow.AddYears(-1), TestShelterId);
        var animal2 = Animal.Create(
            "sig5", "trans5", "Animal2", "Gray", AnimalSpecies.Cat, AnimalSex.Female, DateTimeOffset.UtcNow.AddYears(-2), "other-shelter-id");
        await _repository.AddAsync(animal1);
        await _repository.AddAsync(animal2);

        var list = await _repository.ListAsync(TestShelterId);

        Assert.Single(list);
        Assert.Equal("Animal1", list.First().Name);
    }
}