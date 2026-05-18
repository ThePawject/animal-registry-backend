using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using System.Net;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

[Collection("Sequential")]
public sealed class DateRangeAnimalsReportTests(ApiTestFixture fixture) : IntegrationTestBase(fixture)
{
    private const string TestShelterId = "test-shelter-daterange";

    [Fact]
    public async Task GenerateDateRangeAnimalsReport_ShouldReturnPdf_WhenUserHasShelterAccess()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var client = Factory.CreateAuthenticatedClient(user);
        var factory = new AnimalFactory(new ApiClient(client));

        await factory.CreateAsync("2024/6001", "trans-dog-dr-1", "DoggoDR", "Labrador", "tail", AnimalSpecies.Dog,
            AnimalSex.Male);

        var startDate = DateTimeOffset.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");

        var response =
            await client.GetAsync(
                $"/reports/animals/date-range?startDate={startDate}&endDate={endDate}&species={(int)AnimalSpecies.Dog}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        var contentDisposition = response.Content.Headers.GetValues("Content-Disposition").FirstOrDefault();
        contentDisposition.Should().Contain("attachment");
        contentDisposition.Should().Contain("RaportZwierzatZakresDat_");
        contentDisposition.Should().Contain(".pdf");

        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        pdfBytes.Should().NotBeNullOrEmpty();
        pdfBytes.Length.Should().BeGreaterThan(100);

        // PDF magic number
        pdfBytes[0].Should().Be(0x25);
        pdfBytes[1].Should().Be(0x50);
        pdfBytes[2].Should().Be(0x44);
        pdfBytes[3].Should().Be(0x46);
    }

    [Fact]
    public async Task GenerateDateRangeAnimalsReport_ShouldReturnValidationError_WhenStartDateAfterEndDate()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var client = Factory.CreateAuthenticatedClient(user);

        var startDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        var endDate = DateTimeOffset.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");

        var response = await client.GetAsync($"/reports/animals/date-range?startDate={startDate}&endDate={endDate}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateDateRangeAnimalsReport_ShouldReturnForbidden_WhenUserHasNoShelterAccess()
    {
        var client = Factory.CreateAuthenticatedClient(TestUser.WithoutShelterAccess());

        var startDate = DateTimeOffset.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");

        var response = await client.GetAsync($"/reports/animals/date-range?startDate={startDate}&endDate={endDate}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GenerateDateRangeAnimalsReport_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var startDate = DateTimeOffset.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");

        var response = await Client.GetAsync($"/reports/animals/date-range?startDate={startDate}&endDate={endDate}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateDateRangeAnimalsReport_ShouldFilterBySpecies_WhenSpeciesProvided()
    {
        var user = TestUser.WithShelterAccess("test-shelter-species-filter");
        var client = Factory.CreateAuthenticatedClient(user);
        var factory = new AnimalFactory(new ApiClient(client));

        var dog1 = await factory.CreateAsync("2024/7001", "trans-dog-sf-1", "DoggoSF1", "Labrador", "tail",
            AnimalSpecies.Dog, AnimalSex.Male);
        var dog2 = await factory.CreateAsync("2024/7002", "trans-dog-sf-2", "DoggoSF2", "Beagle", "spot",
            AnimalSpecies.Dog, AnimalSex.Female);
        var cat1 = await factory.CreateAsync("2024/7003", "trans-cat-sf-1", "KittySF1", "Persian", "white paws",
            AnimalSpecies.Cat, AnimalSex.Female);
        var cat2 = await factory.CreateAsync("2024/7004", "trans-cat-sf-2", "KittySF2", "Siamese", "blue eyes",
            AnimalSpecies.Cat, AnimalSex.Male);

        await factory.AddEvent(dog1, AnimalEventType.AdmissionToShelter, DateTimeOffset.UtcNow.AddDays(-5));
        await factory.AddEvent(dog2, AnimalEventType.AdmissionToShelter, DateTimeOffset.UtcNow.AddDays(-4));
        await factory.AddEvent(cat1, AnimalEventType.AdmissionToShelter, DateTimeOffset.UtcNow.AddDays(-5));
        await factory.AddEvent(cat2, AnimalEventType.AdmissionToShelter, DateTimeOffset.UtcNow.AddDays(-3));

        var startDate = DateTimeOffset.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");

        var responseWithDogFilter =
            await client.GetAsync(
                $"/reports/animals/date-range?startDate={startDate}&endDate={endDate}&species={(int)AnimalSpecies.Dog}");
        var responseWithCatFilter =
            await client.GetAsync(
                $"/reports/animals/date-range?startDate={startDate}&endDate={endDate}&species={(int)AnimalSpecies.Cat}");
        var responseWithMultipleSpecies =
            await client.GetAsync(
                $"/reports/animals/date-range?startDate={startDate}&endDate={endDate}&species={(int)AnimalSpecies.Dog}&species={(int)AnimalSpecies.Cat}");

        responseWithDogFilter.StatusCode.Should().Be(HttpStatusCode.OK);
        responseWithCatFilter.StatusCode.Should().Be(HttpStatusCode.OK);
        responseWithMultipleSpecies.StatusCode.Should().Be(HttpStatusCode.OK);

        var dogPdfBytes = await responseWithDogFilter.Content.ReadAsByteArrayAsync();
        var catPdfBytes = await responseWithCatFilter.Content.ReadAsByteArrayAsync();
        var multiplePdfBytes = await responseWithMultipleSpecies.Content.ReadAsByteArrayAsync();

        dogPdfBytes.Should().NotBeNullOrEmpty();
        catPdfBytes.Should().NotBeNullOrEmpty();
        multiplePdfBytes.Should().NotBeNullOrEmpty();

        dogPdfBytes.Length.Should().BeGreaterThan(100);
        catPdfBytes.Length.Should().BeGreaterThan(100);
        multiplePdfBytes.Length.Should().BeGreaterThan(dogPdfBytes.Length);
        multiplePdfBytes.Length.Should().BeGreaterThan(catPdfBytes.Length);
    }
}