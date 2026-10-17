using AnimalRegistry.Modules.Animals.Api.Reports;
using AnimalRegistry.Modules.Animals.Domain;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using FluentAssertions;
using System.Net;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Vaccinations;

[Collection("Sequential")]
public sealed class VaccinationReportsTests(ApiTestFixture fixture) : VaccinationModuleTestBase(fixture)
{
    private static string PeriodRoute(DateTimeOffset from, DateTimeOffset to)
    {
        return $"{GenerateVaccinationsReportRequest.Route}?from={Uri.EscapeDataString(from.ToString("o"))}" +
               $"&to={Uri.EscapeDataString(to.ToString("o"))}";
    }

    private static async Task ShouldBePdfAsync(HttpResponseMessage response, string fileName)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        response.Content.Headers.GetValues("Content-Disposition").Single().Should().Contain(fileName);
        (await response.Content.ReadAsByteArrayAsync()).Take(4).Should().Equal(0x25, 0x50, 0x44, 0x46);
    }

    [Fact]
    public async Task Reports_ShouldReturnForbidden_WhenModuleIsDisabled()
    {
        var expiring = await Worker.GetAsync(GenerateExpiringVaccinationsReportRequest.Route);
        var period = await Worker.GetAsync(PeriodRoute(LocalDay(-30), LocalDay(0)));

        expiring.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        period.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ExpiringReport_ShouldReturnPdf()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");
        await CreateVaccinationAsync(animalId, Body("Rabies", LocalDay(-360), LocalDay(5), "Jan Kowalski"));
        await CreateVaccinationAsync(animalId, Body("DHPPi", LocalDay(-400), LocalDay(-10)));

        var response = await Worker.GetAsync(
            $"{GenerateExpiringVaccinationsReportRequest.Route}?expiresInDays=14&includeExpired=true");

        var today = ShelterCalendar.ToLocal(DateTimeOffset.UtcNow);
        await ShouldBePdfAsync(response, $"KonczaceSieSzczepienia_{today:yyyy-MM-dd}.pdf");
    }

    [Fact]
    public async Task PeriodReport_ShouldReturnPdf()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");
        await CreateVaccinationAsync(animalId, Body("Rabies", LocalDay(-5), LocalDay(360), "Jan", "S1"));

        var response = await Worker.GetAsync(PeriodRoute(LocalDay(-30), LocalDay(0)));

        var from = ShelterCalendar.ToLocal(LocalDay(-30));
        var to = ShelterCalendar.ToLocal(LocalDay(0));
        await ShouldBePdfAsync(response, $"Szczepienia_{from:yyyy-MM-dd}_{to:yyyy-MM-dd}.pdf");
    }

    [Fact]
    public async Task PeriodReport_ShouldReturnBadRequest_WhenDatesAreMissingOrReversed()
    {
        await SetModuleAsync(true);

        var missing = await Worker.GetAsync(GenerateVaccinationsReportRequest.Route);
        var reversed = await Worker.GetAsync(PeriodRoute(LocalDay(0), LocalDay(-1)));

        missing.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        reversed.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExpiringReport_ShouldReturnBadRequest_WhenDaysAreOutOfRange()
    {
        await SetModuleAsync(true);

        var response = await Worker.GetAsync($"{GenerateExpiringVaccinationsReportRequest.Route}?expiresInDays=-1");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
