using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using System.Net;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

public sealed class DateRangeAnimalsReportTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string TestShelterId = "test-shelter-daterange";

    [Fact]
    public async Task GenerateDateRangeAnimalsReport_ShouldReturnPdf_WhenUserHasShelterAccess()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var client = fixture.CreateAuthenticatedClient(user);
        var factory = new AnimalFactory(new ApiClient(client));

        var dogId = await factory.CreateAsync("sig-dog-dr-1", "trans-dog-dr-1", "DoggoDR", AnimalSpecies.Dog, AnimalSex.Male);

        var startDate = DateTimeOffset.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");

        var response = await client.GetAsync($"/reports/animals/date-range?startDate={startDate}&endDate={endDate}&species=Dog");

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
        var client = fixture.CreateAuthenticatedClient(user);

        var startDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        var endDate = DateTimeOffset.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");

        var response = await client.GetAsync($"/reports/animals/date-range?startDate={startDate}&endDate={endDate}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateDateRangeAnimalsReport_ShouldReturnForbidden_WhenUserHasNoShelterAccess()
    {
        var client = fixture.CreateAuthenticatedClient(TestUser.WithoutShelterAccess());

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

        var response = await fixture.Client.GetAsync($"/reports/animals/date-range?startDate={startDate}&endDate={endDate}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
