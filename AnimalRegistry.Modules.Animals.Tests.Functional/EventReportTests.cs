using AnimalRegistry.Modules.Animals.Api.AnimalEvents;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

[Collection("Sequential")]
public sealed class EventReportTests(ApiTestFixture fixture) : IntegrationTestBase(fixture)
{
    private const string TestShelterId = "test-shelter-1";

    [Fact]
    public async Task GenerateEventReport_ShouldReturnPdf_WhenUserHasShelterAccess()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var client = Factory.CreateAuthenticatedClient(user);
        var factory = new AnimalFactory(new ApiClient(client));

        var dogId = await factory.CreateAsync("2024/7001", "trans-dog-1", "Doggo", AnimalSpecies.Dog, AnimalSex.Male);
        var catId = await factory.CreateAsync("2024/7002", "trans-cat-1", "Kitty", AnimalSpecies.Cat, AnimalSex.Female);

        var dogEventRequest = new CreateAnimalEventRequest
        {
            AnimalId = dogId,
            Type = AnimalEventType.Adoption,
            OccurredOn = DateTimeOffset.UtcNow.AddDays(-5),
            Description = "Test adoption",
        };
        var catEventRequest = new CreateAnimalEventRequest
        {
            AnimalId = catId,
            Type = AnimalEventType.Sterilization,
            OccurredOn = DateTimeOffset.UtcNow.AddDays(-3),
            Description = "Test sterilization",
        };

        await client.PostAsJsonAsync(CreateAnimalEventRequest.BuildRoute(dogId), dogEventRequest);
        await client.PostAsJsonAsync(CreateAnimalEventRequest.BuildRoute(catId), catEventRequest);

        var response = await client.GetAsync("/reports/events");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        var contentDisposition = response.Content.Headers.GetValues("Content-Disposition").FirstOrDefault();
        contentDisposition.Should().Contain("attachment");
        contentDisposition.Should().Contain("RaportZdarzen_");
        contentDisposition.Should().Contain(".pdf");

        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        pdfBytes.Should().NotBeNullOrEmpty();
        pdfBytes.Length.Should().BeGreaterThan(100);

        pdfBytes[0].Should().Be(0x25);
        pdfBytes[1].Should().Be(0x50);
        pdfBytes[2].Should().Be(0x44);
        pdfBytes[3].Should().Be(0x46);
    }

    [Fact]
    public async Task GenerateEventReport_ShouldReturnPdf_WithCorrectFilename()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var client = Factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync("/reports/events");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var contentDisposition = response.Content.Headers.GetValues("Content-Disposition").FirstOrDefault();
        contentDisposition.Should().NotBeNull();
        contentDisposition.Should().Match("*filename=\"RaportZdarzen_*.pdf\"*");
    }

    [Fact]
    public async Task GenerateEventReport_ShouldReturnForbidden_WhenUserHasNoShelterAccess()
    {
        var client = Factory.CreateAuthenticatedClient(TestUser.WithoutShelterAccess());

        var response = await client.GetAsync("/reports/events");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GenerateEventReport_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var response = await Client.GetAsync("/reports/events");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateEventReport_ShouldIncludeAllPeriodsInPdf()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var client = Factory.CreateAuthenticatedClient(user);
        var factory = new AnimalFactory(new ApiClient(client));

        var dogId = await factory.CreateAsync("2024/7003", "trans-dog-2", "Doggo2", AnimalSpecies.Dog, AnimalSex.Male);

        await client.PostAsJsonAsync(CreateAnimalEventRequest.BuildRoute(dogId), new CreateAnimalEventRequest
        {
            AnimalId = dogId,
            Type = AnimalEventType.AdmissionToShelter,
            OccurredOn = DateTimeOffset.UtcNow.AddDays(-80),
            Description = "Quarterly event",
        });

        await client.PostAsJsonAsync(CreateAnimalEventRequest.BuildRoute(dogId), new CreateAnimalEventRequest
        {
            AnimalId = dogId,
            Type = AnimalEventType.Adoption,
            OccurredOn = DateTimeOffset.UtcNow.AddDays(-20),
            Description = "Monthly event",
        });

        await client.PostAsJsonAsync(CreateAnimalEventRequest.BuildRoute(dogId), new CreateAnimalEventRequest
        {
            AnimalId = dogId,
            Type = AnimalEventType.Walk,
            OccurredOn = DateTimeOffset.UtcNow.AddDays(-3),
            Description = "Weekly event",
        });

        var response = await client.GetAsync("/reports/events");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        pdfBytes.Should().NotBeNullOrEmpty();
    }
}