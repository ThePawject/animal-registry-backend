using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using System.Net;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

public sealed class SelectedAnimalsReportTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string TestShelterId = "test-shelter-selected";

    [Fact]
    public async Task GenerateSelectedAnimalsReport_ShouldReturnPdf_WhenUserHasShelterAccess()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var client = fixture.CreateAuthenticatedClient(user);
        var factory = new AnimalFactory(new ApiClient(client));

        var dogId = await factory.CreateAsync("sig-dog-sel-1", "trans-dog-sel-1", "DoggoSel", AnimalSpecies.Dog, AnimalSex.Male);
        var catId = await factory.CreateAsync("sig-cat-sel-1", "trans-cat-sel-1", "KittySel", AnimalSpecies.Cat, AnimalSex.Female);

        var response = await client.GetAsync($"/reports/animals/selected?ids={dogId}&ids={catId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        var contentDisposition = response.Content.Headers.GetValues("Content-Disposition").FirstOrDefault();
        contentDisposition.Should().Contain("attachment");
        contentDisposition.Should().Contain("RaportWybranychZwierzat_");
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
    public async Task GenerateSelectedAnimalsReport_ShouldReturnValidationError_WhenNoIdsProvided()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var client = fixture.CreateAuthenticatedClient(user);

        var response = await client.GetAsync("/reports/animals/selected");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateSelectedAnimalsReport_ShouldReturnForbidden_WhenUserHasNoShelterAccess()
    {
        var client = fixture.CreateAuthenticatedClient(TestUser.WithoutShelterAccess());
        var id = Guid.NewGuid();

        var response = await client.GetAsync($"/reports/animals/selected?ids={id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GenerateSelectedAnimalsReport_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var id = Guid.NewGuid();

        var response = await fixture.Client.GetAsync($"/reports/animals/selected?ids={id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
