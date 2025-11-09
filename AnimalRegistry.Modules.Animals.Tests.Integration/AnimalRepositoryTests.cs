using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Infrastructure;
using AnimalRegistry.Modules.Animals.Infrastructure.Animals;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace AnimalRegistry.Modules.Animals.Tests.Integration;

public class AnimalRepositoryTests : IAsyncLifetime
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

    [Fact]
    public async Task AddAndGetAnimal_WorksCorrectly()
    {
        var animal = Animal.Create(
            "sig1", "trans1", "Burek", "Brown", AnimalSpecies.Dog, AnimalSex.Male, DateTimeOffset.UtcNow.AddYears(-2));
        await _repository.AddAsync(animal);
        var loaded = await _repository.GetByIdAsync(animal.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Burek", loaded!.Name);
    }

    [Fact]
    public async Task RemoveAnimal_WorksCorrectly()
    {
        var animal = Animal.Create(
            "sig2", "trans2", "Mruczek", "Gray", AnimalSpecies.Cat, AnimalSex.Female,
            DateTimeOffset.UtcNow.AddYears(-3));
        await _repository.AddAsync(animal);
        _repository.Remove(animal);
        var loaded = await _repository.GetByIdAsync(animal.Id);
        Assert.Null(loaded);
    }
}