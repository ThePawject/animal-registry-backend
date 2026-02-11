using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

[Collection("Sequential")]
public sealed class AnimalSignatureTests(ApiTestFixture fixture) : IntegrationTestBase(fixture)
{
    private static string GetTestShelterId([System.Runtime.CompilerServices.CallerMemberName] string testName = "") => $"test-sig-{testName}";

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = Factory.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    [Fact]
    public async Task GetNextAvailableSignature_ReturnsCurrentYearAndNumberOne_WhenNoAnimals()
    {
        var shelterId = GetTestShelterId();
        var client = Factory.CreateAuthenticatedClient(TestUser.WithShelterAccess(shelterId));
        var currentYear = DateTimeOffset.UtcNow.Year;

        var response = await client.GetAsync("/animals/signature");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetNextAvailableSignatureResponse>();
        result.Should().NotBeNull();
        result!.Signature.Should().Be($"{currentYear}/0001");
    }

    [Fact]
    public async Task GetNextAvailableSignature_ReturnsNextNumber_WhenAnimalsExist()
    {
        var shelterId = GetTestShelterId();
        var factory = CreateFactory(TestUser.WithShelterAccess(shelterId));
        var currentYear = DateTimeOffset.UtcNow.Year;
        
        await factory.CreateAsync($"{currentYear}/0001", "trans-1", "First", AnimalSpecies.Dog, AnimalSex.Male);
        await factory.CreateAsync($"{currentYear}/0003", "trans-3", "Third", AnimalSpecies.Cat, AnimalSex.Female);

        var client = Factory.CreateAuthenticatedClient(TestUser.WithShelterAccess(shelterId));
        var response = await client.GetAsync("/animals/signature");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetNextAvailableSignatureResponse>();
        result.Should().NotBeNull();
        result!.Signature.Should().Be($"{currentYear}/0002");
    }

    [Fact]
    public async Task GetNextAvailableSignature_WithSpecificYear_ReturnsCorrectFormat()
    {
        var shelterId = GetTestShelterId();
        var client = Factory.CreateAuthenticatedClient(TestUser.WithShelterAccess(shelterId));

        var response = await client.GetAsync("/animals/signature?year=2025");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetNextAvailableSignatureResponse>();
        result.Should().NotBeNull();
        result!.Signature.Should().Be("2025/0001");
    }

    [Fact]
    public async Task CreateAnimal_WithValidSignature_Succeeds()
    {
        var shelterId = GetTestShelterId();
        var factory = CreateFactory(TestUser.WithShelterAccess(shelterId));
        var currentYear = DateTimeOffset.UtcNow.Year;

        var id = await factory.CreateAsync($"{currentYear}/0100", "trans-valid", "Valid", AnimalSpecies.Dog, AnimalSex.Male);

        id.Should().NotBe(Guid.Empty);
        var animal = await factory.GetAsync(id);
        animal.Signature.Should().Be($"{currentYear}/0100");
    }

    [Fact]
    public async Task CreateAnimal_WithInvalidFormat_ReturnsValidationError()
    {
        var shelterId = GetTestShelterId();
        var client = Factory.CreateAuthenticatedClient(TestUser.WithShelterAccess(shelterId));
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("invalid-sig"), "Signature");
        content.Add(new StringContent("trans-123"), "TransponderCode");
        content.Add(new StringContent("Test"), "Name");
        content.Add(new StringContent("Brown"), "Color");
        content.Add(new StringContent("1"), "Species");
        content.Add(new StringContent("1"), "Sex");
        content.Add(new StringContent(DateTimeOffset.UtcNow.AddYears(-1).ToString("o")), "BirthDate");

        var response = await client.PostAsync(CreateAnimalRequest.Route, content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAnimal_WithDuplicateSignature_ReturnsValidationError()
    {
        var shelterId = GetTestShelterId();
        var factory = CreateFactory(TestUser.WithShelterAccess(shelterId));
        var currentYear = DateTimeOffset.UtcNow.Year;
        var signature = $"{currentYear}/0200";
        
        await factory.CreateAsync(signature, "trans-dup", "First", AnimalSpecies.Dog, AnimalSex.Male);

        var client = Factory.CreateAuthenticatedClient(TestUser.WithShelterAccess(shelterId));
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(signature), "Signature");
        content.Add(new StringContent("trans-dup-2"), "TransponderCode");
        content.Add(new StringContent("Second"), "Name");
        content.Add(new StringContent("Black"), "Color");
        content.Add(new StringContent("1"), "Species");
        content.Add(new StringContent("1"), "Sex");
        content.Add(new StringContent(DateTimeOffset.UtcNow.AddYears(-1).ToString("o")), "BirthDate");

        var response = await client.PostAsync(CreateAnimalRequest.Route, content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain(signature);
    }

    [Fact]
    public async Task UpdateAnimal_WithDifferentSignature_Succeeds()
    {
        var shelterId = GetTestShelterId();
        var factory = CreateFactory(TestUser.WithShelterAccess(shelterId));
        var currentYear = DateTimeOffset.UtcNow.Year;
        var id = await factory.CreateAsync($"{currentYear}/0300", "trans-update", "Before", AnimalSpecies.Dog, AnimalSex.Male);

        await factory.UpdateAsync(id, $"{currentYear}/0301", "trans-update", "After", AnimalSpecies.Dog, AnimalSex.Male, []);

        var animal = await factory.GetAsync(id);
        animal.Signature.Should().Be($"{currentYear}/0301");
    }

    [Fact]
    public async Task UpdateAnimal_WithDuplicateSignature_ReturnsValidationError()
    {
        var shelterId = GetTestShelterId();
        var factory = CreateFactory(TestUser.WithShelterAccess(shelterId));
        var currentYear = DateTimeOffset.UtcNow.Year;
        var id1 = await factory.CreateAsync($"{currentYear}/0400", "trans-1", "First", AnimalSpecies.Dog, AnimalSex.Male);
        await factory.CreateAsync($"{currentYear}/0401", "trans-2", "Second", AnimalSpecies.Cat, AnimalSex.Female);

        var client = Factory.CreateAuthenticatedClient(TestUser.WithShelterAccess(shelterId));
        var content = new MultipartFormDataContent();
        content.Add(new StringContent($"{currentYear}/0401"), "Signature");
        content.Add(new StringContent("trans-1-updated"), "TransponderCode");
        content.Add(new StringContent("Updated"), "Name");
        content.Add(new StringContent("Gray"), "Color");
        content.Add(new StringContent("1"), "Species");
        content.Add(new StringContent("1"), "Sex");
        content.Add(new StringContent(DateTimeOffset.UtcNow.AddYears(-2).ToString("o")), "BirthDate");

        var response = await client.PutAsync(UpdateAnimalRequest.BuildRoute(id1), content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetNextAvailableSignature_ReturnsLowestAvailableNumber_WithGaps()
    {
        var shelterId = GetTestShelterId();
        var factory = CreateFactory(TestUser.WithShelterAccess(shelterId));
        var currentYear = DateTimeOffset.UtcNow.Year;
        
        await factory.CreateAsync($"{currentYear}/0001", "trans-1", "One", AnimalSpecies.Dog, AnimalSex.Male);
        await factory.CreateAsync($"{currentYear}/0002", "trans-2", "Two", AnimalSpecies.Cat, AnimalSex.Female);
        await factory.CreateAsync($"{currentYear}/0004", "trans-4", "Four", AnimalSpecies.Dog, AnimalSex.Male);

        var client = Factory.CreateAuthenticatedClient(TestUser.WithShelterAccess(shelterId));
        var response = await client.GetAsync("/animals/signature");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetNextAvailableSignatureResponse>();
        result.Should().NotBeNull();
        result!.Signature.Should().Be($"{currentYear}/0003");
    }

    [Fact]
    public async Task SignaturesAreUniquePerShelter_DifferentSheltersCanHaveSameSignature()
    {
        var shelter1 = "test-shelter-sig-A";
        var shelter2 = "test-shelter-sig-B";
        var factory1 = CreateFactory(TestUser.WithShelterAccess(shelter1));
        var factory2 = CreateFactory(TestUser.WithShelterAccess(shelter2));
        var currentYear = DateTimeOffset.UtcNow.Year;
        var signature = $"{currentYear}/0500";

        var id1 = await factory1.CreateAsync(signature, "trans-a", "AnimalA", AnimalSpecies.Dog, AnimalSex.Male);
        var id2 = await factory2.CreateAsync(signature, "trans-b", "AnimalB", AnimalSpecies.Cat, AnimalSex.Female);

        var animal1 = await factory1.GetAsync(id1);
        var animal2 = await factory2.GetAsync(id2);
        
        animal1.Signature.Should().Be(signature);
        animal2.Signature.Should().Be(signature);
    }
}
