using AnimalRegistry.Modules.Animals.Api.Vaccinations;
using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Shared.Testing;
using AnimalRegistry.Shared.Pagination;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Vaccinations;

[Collection("Sequential")]
public sealed class VaccinationReplacementTests(ApiTestFixture fixture) : VaccinationModuleTestBase(fixture)
{
    private async Task<List<VaccinationWithAnimalDto>> ExpiringAsync(string query = "")
    {
        var page = await Worker.GetFromJsonAsync<PagedResult<VaccinationWithAnimalDto>>(
            $"{ListExpiringVaccinationsRequest.Route}{query}");
        return page!.Items.ToList();
    }

    private async Task<(Guid AnimalId, AnimalVaccinationDto Old)> CreateExpiredAsync()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");
        var old = await CreateVaccinationAsync(animalId, Body("Rabies", LocalDay(-400), LocalDay(-5)));
        return (animalId, old);
    }

    [Fact]
    public async Task Create_ShouldMarkReplacedVaccination()
    {
        var (animalId, old) = await CreateExpiredAsync();

        var renewed = await CreateVaccinationAsync(animalId,
            Body("Rabies", LocalDay(-1), LocalDay(364), replacesVaccinationId: old.Id));

        renewed.ReplacesVaccinationId.Should().Be(old.Id);
        renewed.Status.Should().Be(VaccinationStatus.Active);
        var vaccinations = await ListAsync(animalId);
        var oldAfter = vaccinations.Single(v => v.Id == old.Id);
        oldAfter.Status.Should().Be(VaccinationStatus.Replaced);
        oldAfter.ReplacedByVaccinationId.Should().Be(renewed.Id);
        vaccinations.Single(v => v.Id == renewed.Id).ReplacedByVaccinationId.Should().BeNull();
    }

    [Fact]
    public async Task ExpiringList_ShouldSkipReplacedVaccinations_EvenWhenExpiredAreIncluded()
    {
        var (animalId, old) = await CreateExpiredAsync();
        await CreateVaccinationAsync(animalId,
            Body("Rabies", LocalDay(-1), LocalDay(364), replacesVaccinationId: old.Id));

        var expiring = await ExpiringAsync("?expiresInDays=30&includeExpired=true");

        expiring.Should().BeEmpty();
    }

    [Fact]
    public async Task ExpiringList_ShouldSkipVaccinationRenewedBeforeItExpired()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");
        var old = await CreateVaccinationAsync(animalId, Body("Rabies", LocalDay(-360), LocalDay(5)));
        await CreateVaccinationAsync(animalId,
            Body("Rabies", LocalDay(-1), LocalDay(364), replacesVaccinationId: old.Id));

        var expiring = await ExpiringAsync();

        expiring.Should().BeEmpty();
        (await ListAsync(animalId)).Single(v => v.Id == old.Id).Status.Should().Be(VaccinationStatus.Replaced);
    }

    [Fact]
    public async Task DeleteReplacingVaccination_ShouldRestoreOldStatus()
    {
        var (animalId, old) = await CreateExpiredAsync();
        var renewed = await CreateVaccinationAsync(animalId,
            Body("Rabies", LocalDay(-1), replacesVaccinationId: old.Id));

        (await Worker.DeleteAsync(DeleteAnimalVaccinationRequest.BuildRoute(animalId, renewed.Id)))
            .EnsureSuccessStatusCode();

        var oldAfter = (await ListAsync(animalId)).Single();
        oldAfter.Status.Should().Be(VaccinationStatus.Expired);
        oldAfter.ReplacedByVaccinationId.Should().BeNull();
    }

    [Fact]
    public async Task DeleteReplacedVaccination_ShouldUnlinkReplacingOne()
    {
        var (animalId, old) = await CreateExpiredAsync();
        var renewed = await CreateVaccinationAsync(animalId,
            Body("Rabies", LocalDay(-1), replacesVaccinationId: old.Id));

        var response = await Worker.DeleteAsync(DeleteAnimalVaccinationRequest.BuildRoute(animalId, old.Id));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var remaining = (await ListAsync(animalId)).Single();
        remaining.Id.Should().Be(renewed.Id);
        remaining.ReplacesVaccinationId.Should().BeNull();
    }

    [Fact]
    public async Task Update_ShouldSetAndClearReplacement()
    {
        var (animalId, old) = await CreateExpiredAsync();
        var renewed = await CreateVaccinationAsync(animalId, Body("Rabies", LocalDay(-1)));
        var route = UpdateAnimalVaccinationRequest.BuildRoute(animalId, renewed.Id);

        var set = await Worker.PutAsJsonAsync(route, Body("Rabies", LocalDay(-1), replacesVaccinationId: old.Id));
        set.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ListAsync(animalId)).Single(v => v.Id == old.Id).Status.Should().Be(VaccinationStatus.Replaced);

        var unchanged = await Worker.PutAsJsonAsync(route,
            Body("Rabies", LocalDay(-1), notes: "Bez zmian", replacesVaccinationId: old.Id));
        unchanged.StatusCode.Should().Be(HttpStatusCode.OK);

        var cleared = await Worker.PutAsJsonAsync(route, Body("Rabies", LocalDay(-1)));
        cleared.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ListAsync(animalId)).Single(v => v.Id == old.Id).Status.Should().Be(VaccinationStatus.Expired);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenReplacedVaccinationIsOfAnotherAnimalOrShelter()
    {
        var (_, old) = await CreateExpiredAsync();
        var otherAnimalId = await CreateAnimalAsync("2026/0002");
        var otherAdmin = await CreateOtherShelterAdminWithModuleAsync();
        var foreignAnimalId = await CreateAnimalAsync("2026/0003", new AnimalFactory(new ApiClient(otherAdmin)));
        var foreign = await CreateVaccinationAsync(foreignAnimalId, Body("Rabies", LocalDay(-400)), otherAdmin);

        var otherAnimal = await PostVaccinationAsync(Worker, otherAnimalId, Body(replacesVaccinationId: old.Id));
        var otherShelter = await PostVaccinationAsync(Worker, otherAnimalId, Body(replacesVaccinationId: foreign.Id));
        var unknown = await PostVaccinationAsync(Worker, otherAnimalId, Body(replacesVaccinationId: Guid.NewGuid()));

        otherAnimal.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        otherShelter.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        unknown.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenReplacedVaccinationIsNotEarlier()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");
        var recent = await CreateVaccinationAsync(animalId, Body("Rabies", LocalDay(-1)));

        var response = await PostVaccinationAsync(Worker, animalId,
            Body("Rabies", LocalDay(-10), replacesVaccinationId: recent.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenVaccinationIsAlreadyReplaced()
    {
        var (animalId, old) = await CreateExpiredAsync();
        await CreateVaccinationAsync(animalId, Body("Rabies", LocalDay(-2), replacesVaccinationId: old.Id));

        var response = await PostVaccinationAsync(Worker, animalId,
            Body("Rabies", LocalDay(-1), replacesVaccinationId: old.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenVaccinationReplacesItself()
    {
        var (animalId, old) = await CreateExpiredAsync();

        var response = await Worker.PutAsJsonAsync(UpdateAnimalVaccinationRequest.BuildRoute(animalId, old.Id),
            Body("Rabies", LocalDay(-400), replacesVaccinationId: old.Id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
