using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using FluentAssertions;
using System.Net;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

[Collection("Sequential")]
public sealed class RepositoryDumpReportTests(ApiTestFixture fixture) : IntegrationTestBase(fixture)
{
    private const string TestShelterId = "test-shelter-dump";

    [Fact]
    public async Task GenerateRepositoryDumpReport_ShouldReturnPdf_WhenUserHasShelterAccess()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var client = Factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync("/reports/animals/dump");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        var contentDisposition = response.Content.Headers.GetValues("Content-Disposition").FirstOrDefault();
        contentDisposition.Should().Contain("attachment");
        contentDisposition.Should().Contain("ZrzutRepozytorium_");
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
    public async Task GenerateRepositoryDumpReport_ShouldReturnForbidden_WhenUserHasNoShelterAccess()
    {
        var client = Factory.CreateAuthenticatedClient(TestUser.WithoutShelterAccess());

        var response = await client.GetAsync("/reports/animals/dump");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GenerateRepositoryDumpReport_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var response = await Client.GetAsync("/reports/animals/dump");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
