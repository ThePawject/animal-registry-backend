using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Modules.Animals.Infrastructure;
using AnimalRegistry.Modules.Animals.Infrastructure.Animals;
using AnimalRegistry.Shared.DDD;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.MsSql;

namespace AnimalRegistry.Modules.Animals.Tests.Integration;

public sealed class AnimalRepositoryTests : IAsyncLifetime
{
    private const string TestShelterId = "test-shelter-id";

    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("yourStrong(!)Password")
        .Build();

    private AnimalsDbContext _dbContext = null!;
    private AnimalRepository _repository = null!;
    private IBlobStorageService _blobStorageService = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        var options = new DbContextOptionsBuilder<AnimalsDbContext>()
            .UseSqlServer(_dbContainer.GetConnectionString())
            .Options;
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        _dbContext = new AnimalsDbContext(options, dispatcher);
        _blobStorageService = Substitute.For<IBlobStorageService>();
        _blobStorageService.GetBlobUrl(Arg.Any<string>()).Returns(x => $"http://test/{x.Arg<string>()}");
        await _dbContext.Database.MigrateAsync();
        _repository = new AnimalRepository(_dbContext, _blobStorageService);
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    [Fact]
    public async Task AddAndGetAnimal_WorksCorrectly()
    {
        var animal = Animal.Create(
            "sig1", "trans1", "Burek", "Brown", AnimalSpecies.Dog, AnimalSex.Male, DateTimeOffset.UtcNow.AddYears(-2),
            TestShelterId);
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
        await _repository.RemoveAsync(animal);
        var loaded = await _repository.GetByIdAsync(animal.Id, TestShelterId);
        Assert.Null(loaded);
    }

    [Fact]
    public async Task GetByIdAsync_WithWrongShelterId_ReturnsNull()
    {
        var animal = Animal.Create(
            "sig3", "trans3", "Reksio", "Black", AnimalSpecies.Dog, AnimalSex.Male, DateTimeOffset.UtcNow.AddYears(-1),
            TestShelterId);
        await _repository.AddAsync(animal);
        var loaded = await _repository.GetByIdAsync(animal.Id, "wrong-shelter-id");
        Assert.Null(loaded);
    }

    [Fact]
    public async Task ListAsync_WithShelterId_ReturnsOnlyMatchingAnimals()
    {
        var animal1 = Animal.Create(
            "sig4", "trans4", "Animal1", "Brown", AnimalSpecies.Dog, AnimalSex.Male, DateTimeOffset.UtcNow.AddYears(-1),
            TestShelterId);
        var animal2 = Animal.Create(
            "sig5", "trans5", "Animal2", "Gray", AnimalSpecies.Cat, AnimalSex.Female,
            DateTimeOffset.UtcNow.AddYears(-2), "other-shelter-id");
        await _repository.AddAsync(animal1);
        await _repository.AddAsync(animal2);

        var result = await _repository.ListAsync(TestShelterId, 1, 20);

        Assert.Single(result.Items);
        Assert.Equal("Animal1", result.Items.First().Name);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task AddEvent_WithCorrectEvent_AddsEvent()
    {
        var animal1 = Animal.Create(
            "sig4", "trans4", "Animal1", "Brown", AnimalSpecies.Dog, AnimalSex.Male, DateTimeOffset.UtcNow.AddYears(-1),
            TestShelterId);
        animal1.AddEvent(AnimalEventType.AdmissionToShelter, TimeProvider.System.GetUtcNow(), "description",
            "performedBy");
        await _repository.AddAsync(animal1);

        var result = await _repository.ListAsync(TestShelterId, 1, 20);

        Assert.Single(result.Items);
        Assert.Equal("Animal1", result.Items.First().Name);
        Assert.Equal(AnimalEventType.AdmissionToShelter, result.Items.First().Events.First().Type);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task UpdateEvent_WithCorrectEvent_UpdatesEvent()
    {
        var animal1 = Animal.Create(
            "sig4", "trans4", "Animal1", "Brown", AnimalSpecies.Dog, AnimalSex.Male, DateTimeOffset.UtcNow.AddYears(-1),
            TestShelterId);
        animal1.AddEvent(AnimalEventType.AdmissionToShelter, TimeProvider.System.GetUtcNow(), "description",
            "performedBy");
        await _repository.AddAsync(animal1);

        animal1.UpdateEvent(animal1.Events.First().Id, AnimalEventType.StartOfQuarantine,
            TimeProvider.System.GetUtcNow(),
            "new description");
        await _repository.UpdateAsync(animal1);

        var result = await _repository.ListAsync(TestShelterId, 1, 20);
        Assert.Single(result.Items);
        Assert.Equal(AnimalEventType.StartOfQuarantine, result.Items.First().Events.First().Type);
    }

    [Fact]
    public async Task RemoveEvent_WithCorrectEvent_RemovesEvent()
    {
        var animal1 = Animal.Create(
            "sig4", "trans4", "Animal1", "Brown", AnimalSpecies.Dog, AnimalSex.Male, DateTimeOffset.UtcNow.AddYears(-1),
            TestShelterId);
        animal1.AddEvent(AnimalEventType.AdmissionToShelter, TimeProvider.System.GetUtcNow(), "description",
            "performedBy");
        await _repository.AddAsync(animal1);
        await _repository.ListAsync(TestShelterId, 1, 20);

        animal1.RemoveEvent(animal1.Events.First().Id);
        await _repository.UpdateAsync(animal1);

        var result = await _repository.ListAsync(TestShelterId, 1, 20);
        Assert.Single(result.Items);
        Assert.Empty(result.Items.First().Events);
    }
}