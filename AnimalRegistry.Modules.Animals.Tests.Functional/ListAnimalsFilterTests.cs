using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

[Collection("Sequential")]
public sealed class ListAnimalsFilterTests(ApiTestFixture fixture) : IntegrationTestBase(fixture)
{
    private const string TestShelterId = "test-shelter-filter";
    private static int _signatureCounter = 5000;

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = Factory.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    private string NextSig()
    {
        return $"2024/{_signatureCounter++:D4}";
    }

    [Fact]
    public async Task List_FilterBySpecies_Dog_ReturnsOnlyDogs()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var dogId1 = await factory.CreateAsync(NextSig(), "dog-1", "Rex", AnimalSpecies.Dog, AnimalSex.Male);
        var dogId2 = await factory.CreateAsync(NextSig(), "dog-2", "Burek", AnimalSpecies.Dog, AnimalSex.Female);
        var catId = await factory.CreateAsync(NextSig(), "cat-1", "Mruczek", AnimalSpecies.Cat, AnimalSex.Male);

        var result = await factory.ListAsync(species: AnimalSpecies.Dog);

        result.Items.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Items.Should().Contain(a => a.Id == dogId1 && a.Name == "Rex");
        result.Items.Should().Contain(a => a.Id == dogId2 && a.Name == "Burek");
        result.Items.Should().NotContain(a => a.Id == catId);
        result.Items.Should().OnlyContain(a => a.Species == AnimalSpecies.Dog);
    }

    [Fact]
    public async Task List_FilterBySpecies_Cat_ReturnsOnlyCats()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var dogId = await factory.CreateAsync(NextSig(), "dog-3", "Azor", AnimalSpecies.Dog, AnimalSex.Male);
        var catId1 = await factory.CreateAsync(NextSig(), "cat-2", "Filemon", AnimalSpecies.Cat, AnimalSex.Male);
        var catId2 = await factory.CreateAsync(NextSig(), "cat-3", "Bonifacy", AnimalSpecies.Cat, AnimalSex.Male);

        var result = await factory.ListAsync(species: AnimalSpecies.Cat);

        result.Items.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Items.Should().Contain(a => a.Id == catId1 && a.Name == "Filemon");
        result.Items.Should().Contain(a => a.Id == catId2 && a.Name == "Bonifacy");
        result.Items.Should().NotContain(a => a.Id == dogId);
        result.Items.Should().OnlyContain(a => a.Species == AnimalSpecies.Cat);
    }

    [Fact]
    public async Task List_FilterBySpecies_Null_ReturnsAllSpecies()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var dogId = await factory.CreateAsync(NextSig(), "dog-4", "Reksio", AnimalSpecies.Dog, AnimalSex.Male);
        var catId = await factory.CreateAsync(NextSig(), "cat-4", "Puszek", AnimalSpecies.Cat, AnimalSex.Female);

        var result = await factory.ListAsync(species: null);

        result.Items.Should().Contain(a => a.Id == dogId && a.Name == "Reksio");
        result.Items.Should().Contain(a => a.Id == catId && a.Name == "Puszek");
    }

    [Fact]
    public async Task List_FilterByIsInShelter_True_ReturnsOnlyAnimalsInShelter()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var inShelterId1 = await factory.CreateAsync(NextSig(), "in-1", "InShelter1", AnimalSpecies.Dog,
            AnimalSex.Male);
        var inShelterId2 = await factory.CreateAsync(NextSig(), "in-2", "InShelter2", AnimalSpecies.Cat,
            AnimalSex.Female);

        var result = await factory.ListAsync(isInShelter: true);

        result.Items.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Items.Should().Contain(a => a.Id == inShelterId1);
        result.Items.Should().Contain(a => a.Id == inShelterId2);
        result.Items.Should().OnlyContain(a => a.IsInShelter == true);
    }

    [Fact]
    public async Task List_FilterByIsInShelter_False_ReturnsOnlyAnimalsNotInShelter()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var animalId = await factory.CreateAsync(NextSig(), "out-1", "NotInShelter", AnimalSpecies.Dog,
            AnimalSex.Male);

        var result = await factory.ListAsync(isInShelter: false);

        result.Items.Should().NotContain(a => a.Id == animalId);
        result.Items.Should().OnlyContain(a => a.IsInShelter == false);
    }

    [Fact]
    public async Task List_FilterByIsInShelter_Null_ReturnsAllAnimals()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var id1 = await factory.CreateAsync(NextSig(), "all-1", "Animal1", AnimalSpecies.Dog, AnimalSex.Male);
        var id2 = await factory.CreateAsync(NextSig(), "all-2", "Animal2", AnimalSpecies.Cat, AnimalSex.Female);

        var result = await factory.ListAsync(isInShelter: null);

        result.Items.Should().Contain(a => a.Id == id1);
        result.Items.Should().Contain(a => a.Id == id2);
    }

    [Fact]
    public async Task List_CombineSpeciesAndIsInShelter_ReturnsCorrectResults()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var dogInShelterId = await factory.CreateAsync(NextSig(), "combo-1", "DogInShelter", AnimalSpecies.Dog,
            AnimalSex.Male);
        var catInShelterId = await factory.CreateAsync(NextSig(), "combo-2", "CatInShelter", AnimalSpecies.Cat,
            AnimalSex.Female);

        var result = await factory.ListAsync(species: AnimalSpecies.Dog, isInShelter: true);

        result.Items.Should().Contain(a => a.Id == dogInShelterId);
        result.Items.Should().NotContain(a => a.Id == catInShelterId);
        result.Items.Should().OnlyContain(a => a.Species == AnimalSpecies.Dog && a.IsInShelter == true);
    }

    [Fact]
    public async Task List_CombineFiltersWithKeywordSearch_ReturnsCorrectResults()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var dogId = await factory.CreateAsync(NextSig(), "search-dog-1", "SearchDog", AnimalSpecies.Dog,
            AnimalSex.Male);
        await factory.CreateAsync(NextSig(), "search-cat-1", "SearchCat", AnimalSpecies.Cat, AnimalSex.Female);
        await factory.CreateAsync(NextSig(), "other-dog-1", "OtherDog", AnimalSpecies.Dog, AnimalSex.Male);

        var result = await factory.ListAsync(keyWordSearch: "SearchDog", species: AnimalSpecies.Dog,
            isInShelter: true);

        result.Items.Should().ContainSingle(a => a.Id == dogId && a.Name == "SearchDog");
        result.Items.Should().OnlyContain(a => a.Species == AnimalSpecies.Dog && a.IsInShelter == true);
    }

    [Fact]
    public async Task List_NoFilters_ReturnsAllAnimals()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var dogId = await factory.CreateAsync(NextSig(), "no-filter-1", "NoFilterDog", AnimalSpecies.Dog,
            AnimalSex.Male);
        var catId = await factory.CreateAsync(NextSig(), "no-filter-2", "NoFilterCat", AnimalSpecies.Cat,
            AnimalSex.Female);

        var result = await factory.ListAsync();

        result.Items.Should().Contain(a => a.Id == dogId);
        result.Items.Should().Contain(a => a.Id == catId);
    }

    [Fact]
    public async Task List_FilterBySpecies_WithPagination_ReturnsCorrectPage()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var dogIds = new List<Guid>();
        for (var i = 0; i < 5; i++)
        {
            var id = await factory.CreateAsync(NextSig(), $"page-dog-{i}", $"PageDog{i}", AnimalSpecies.Dog,
                AnimalSex.Male);
            dogIds.Add(id);
        }

        var page1 = await factory.ListAsync(species: AnimalSpecies.Dog, page: 1, pageSize: 2);

        page1.Items.Should().HaveCount(2);
        page1.PageSize.Should().Be(2);
        page1.Page.Should().Be(1);
        page1.TotalCount.Should().BeGreaterThanOrEqualTo(5);
        page1.Items.Should().OnlyContain(a => a.Species == AnimalSpecies.Dog);

        var page2 = await factory.ListAsync(species: AnimalSpecies.Dog, page: 2, pageSize: 2);

        page2.Items.Should().HaveCount(2);
        page2.Page.Should().Be(2);
        page2.Items.Should().OnlyContain(a => a.Species == AnimalSpecies.Dog);

        var page1Ids = page1.Items.Select(a => a.Id).ToList();
        var page2Ids = page2.Items.Select(a => a.Id).ToList();
        page1Ids.Should().NotIntersectWith(page2Ids);
    }
}
