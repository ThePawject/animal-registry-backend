using AnimalRegistry.Modules.Animals.Api.Vaccinations;
using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Vaccinations;

[Collection("Sequential")]
public sealed class ExpiringVaccinationsTests(ApiTestFixture fixture) : VaccinationModuleTestBase(fixture)
{
    private async Task<List<VaccinationWithAnimalDto>> ExpiringAsync(string query = "")
    {
        return (await Worker.GetFromJsonAsync<List<VaccinationWithAnimalDto>>(
            $"{ListExpiringVaccinationsRequest.Route}{query}"))!;
    }

    private async Task<Guid> SeedAsync()
    {
        var animalId = await CreateAnimalAsync("2026/0001");
        await CreateVaccinationAsync(animalId, Body("Expired", LocalDay(-400), LocalDay(-1)));
        await CreateVaccinationAsync(animalId, Body("Today", LocalDay(-365), LocalDay(0)));
        await CreateVaccinationAsync(animalId, Body("In 7 days", LocalDay(-358), LocalDay(7)));
        await CreateVaccinationAsync(animalId, Body("In 30 days", LocalDay(-335), LocalDay(30)));
        await CreateVaccinationAsync(animalId, Body("In 31 days", LocalDay(-334), LocalDay(31)));
        await CreateVaccinationAsync(animalId, Body("Without validity"));
        return animalId;
    }

    [Fact]
    public async Task List_ShouldReturnVaccinationsEndingWithin30Days_ByDefault()
    {
        await SetModuleAsync(true);
        await SeedAsync();

        var vaccinations = await ExpiringAsync();

        vaccinations.Select(v => v.Vaccination.VaccineName).Should().Equal("Today", "In 7 days", "In 30 days");
        vaccinations.Should().OnlyContain(v => v.Vaccination.Status == VaccinationStatus.Active);
        vaccinations[1].Vaccination.DaysUntilExpiry.Should().Be(7);
        vaccinations[0].AnimalSignature.Should().Be("2026/0001");
        vaccinations[0].AnimalName.Should().Be("Burek");
        vaccinations[0].IsInShelter.Should().BeTrue();
    }

    [Fact]
    public async Task List_ShouldRespectDaysAndIncludeExpired()
    {
        await SetModuleAsync(true);
        await SeedAsync();

        var withinWeek = await ExpiringAsync("?expiresInDays=7");
        var withExpired = await ExpiringAsync("?expiresInDays=7&includeExpired=true");

        withinWeek.Select(v => v.Vaccination.VaccineName).Should().Equal("Today", "In 7 days");
        withExpired.Select(v => v.Vaccination.VaccineName).Should().Equal("Expired", "Today", "In 7 days");
        withExpired[0].Vaccination.Status.Should().Be(VaccinationStatus.Expired);
    }

    [Fact]
    public async Task List_ShouldSkipAnimalsOutsideShelter_UnlessRequested()
    {
        await SetModuleAsync(true);
        var adoptedId = await CreateAnimalAsync("2026/0002");
        await CreateVaccinationAsync(adoptedId, Body("Adopted", LocalDay(-300), LocalDay(3)));
        await Animals.AddEvent(adoptedId, AnimalEventType.Adoption);

        var inShelter = await ExpiringAsync();
        var all = await ExpiringAsync("?inShelterOnly=false");

        inShelter.Should().BeEmpty();
        all.Should().ContainSingle().Which.IsInShelter.Should().BeFalse();
    }

    [Fact]
    public async Task List_ShouldReturnOnlyOwnShelterVaccinations()
    {
        await SetModuleAsync(true);
        await SeedAsync();
        var otherAdmin = await CreateOtherShelterAdminWithModuleAsync();
        var otherAnimalId = await CreateAnimalAsync("2026/0009", new AnimalFactory(new ApiClient(otherAdmin)));
        await CreateVaccinationAsync(otherAnimalId, Body("Other shelter", LocalDay(-300), LocalDay(1)), otherAdmin);

        var vaccinations = await ExpiringAsync();

        vaccinations.Should().NotContain(v => v.Vaccination.VaccineName == "Other shelter");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3651)]
    public async Task List_ShouldReturnBadRequest_WhenDaysAreOutOfRange(int days)
    {
        await SetModuleAsync(true);

        var response = await Worker.GetAsync($"{ListExpiringVaccinationsRequest.Route}?expiresInDays={days}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
