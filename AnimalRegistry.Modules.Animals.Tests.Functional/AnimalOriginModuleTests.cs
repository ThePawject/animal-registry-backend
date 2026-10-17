using AnimalRegistry.Modules.Animals.Api.Municipalities;
using AnimalRegistry.Modules.Animals.Api.Reports;
using AnimalRegistry.Modules.Animals.Application.Municipalities;
using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Modules.Shelters.Api;
using AnimalRegistry.Modules.Shelters.Application;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

[Collection("Sequential")]
public sealed class AnimalOriginModuleTests(ApiTestFixture fixture) : IntegrationTestBase(fixture)
{
    private readonly string _shelterId = $"test-shelter-origin-{Guid.NewGuid():N}";
    private const string FeatureKey = "animal-origin";

    private static readonly DateTimeOffset AdmissionDate = new(2026, 3, 15, 10, 0, 0, TimeSpan.FromHours(1));

    [Fact]
    public async Task GetShelterFeatures_ShouldListOriginModuleAsDisabled_ByDefault()
    {
        var client = Factory.CreateAuthenticatedClient(TestUser.WithShelterAccess(_shelterId));

        var features = await client.GetFromJsonAsync<List<ShelterFeatureDto>>(GetShelterFeatures.Route);

        features.Should().ContainSingle(f => f.Key == FeatureKey)
            .Which.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task SetShelterFeatureState_ShouldReturnForbidden_WhenUserIsNotShelterAdmin()
    {
        var client = Factory.CreateAuthenticatedClient(TestUser.WithShelterAccess(_shelterId));

        var response = await client.PutAsJsonAsync(SetShelterFeatureStateRequest.BuildRoute(FeatureKey),
            new { IsEnabled = true });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SetShelterFeatureState_ShouldReturnNotFound_WhenModuleDoesNotExist()
    {
        var client = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(_shelterId));

        var response = await client.PutAsJsonAsync(SetShelterFeatureStateRequest.BuildRoute("unknown-module"),
            new { IsEnabled = true });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetShelterFeatureState_ShouldEnableModuleOnlyForOwnShelter()
    {
        var admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(_shelterId));
        var otherShelter = Factory.CreateAuthenticatedClient(TestUser.WithShelterAccess("other-shelter"));

        await SetModuleAsync(admin, true);

        var own = await admin.GetFromJsonAsync<List<ShelterFeatureDto>>(GetShelterFeatures.Route);
        var other = await otherShelter.GetFromJsonAsync<List<ShelterFeatureDto>>(GetShelterFeatures.Route);
        own!.Single(f => f.Key == FeatureKey).IsEnabled.Should().BeTrue();
        other!.Single(f => f.Key == FeatureKey).IsEnabled.Should().BeFalse();

        await SetModuleAsync(admin, false);

        var disabled = await admin.GetFromJsonAsync<List<ShelterFeatureDto>>(GetShelterFeatures.Route);
        disabled!.Single(f => f.Key == FeatureKey).IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task ModuleEndpoints_ShouldReturnForbidden_WhenModuleIsDisabled()
    {
        var admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(_shelterId));

        var list = await admin.GetAsync(ListMunicipalitiesRequest.Route);
        var create = await admin.PostAsJsonAsync(CreateMunicipalityRequest.Route, new { Name = "Gmina" });
        var monthly = await admin.GetAsync($"{GenerateOriginMonthlyReportRequest.Route}?year=2026&month=3");
        var yearly = await admin.GetAsync($"{GenerateOriginYearlyReportRequest.Route}?year=2026");

        list.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        create.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        monthly.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        yearly.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Municipalities_ShouldBeManagedOnlyByShelterAdmin()
    {
        var admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(_shelterId));
        var worker = Factory.CreateAuthenticatedClient(TestUser.WithShelterAccess(_shelterId));
        await SetModuleAsync(admin, true);

        var forbidden = await worker.PostAsJsonAsync(CreateMunicipalityRequest.Route, new { Name = "Gmina Wiejska" });
        forbidden.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var created = await CreateMunicipalityAsync(admin, "Gmina Wiejska");
        var duplicate = await admin.PostAsJsonAsync(CreateMunicipalityRequest.Route, new { Name = "Gmina Wiejska" });
        duplicate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var renamed = await admin.PutAsJsonAsync(UpdateMunicipalityRequest.BuildRoute(created.Id),
            new { Name = "Gmina Miejska" });
        renamed.StatusCode.Should().Be(HttpStatusCode.OK);

        var archived = await admin.DeleteAsync(SetMunicipalityArchivedRequest.BuildArchiveRoute(created.Id));
        archived.StatusCode.Should().Be(HttpStatusCode.OK);

        var active = await worker.GetFromJsonAsync<List<MunicipalityDto>>(ListMunicipalitiesRequest.Route);
        var all = await worker.GetFromJsonAsync<List<MunicipalityDto>>(
            $"{ListMunicipalitiesRequest.Route}?includeArchived=true");
        active.Should().BeEmpty();
        all.Should().ContainSingle().Which.Should().Match<MunicipalityDto>(m => m.Name == "Gmina Miejska" && m.IsArchived);

        var restored = await admin.PostAsync(SetMunicipalityArchivedRequest.BuildRestoreRoute(created.Id), null);
        restored.StatusCode.Should().Be(HttpStatusCode.OK);
        (await worker.GetFromJsonAsync<List<MunicipalityDto>>(ListMunicipalitiesRequest.Route)).Should().ContainSingle();
    }

    [Fact]
    public async Task CreateAnimal_ShouldRequireOrigin_WhenModuleIsEnabled()
    {
        var admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(_shelterId));
        await SetModuleAsync(admin, true);
        var animals = new AnimalFactory(new ApiClient(admin));

        var response = await animals.PostCreateAsync("2026/0001", "t-1", "Burek", "Mix", "none", AnimalSpecies.Dog,
            AnimalSex.Male);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAnimal_ShouldRejectIncompleteOrigin()
    {
        var admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(_shelterId));
        await SetModuleAsync(admin, true);
        var municipality = await CreateMunicipalityAsync(admin, "Gmina A");
        var animals = new AnimalFactory(new ApiClient(admin));

        var fields = OriginFields(AdmissionMethod.Abandoned, "Leśna 1", municipality.Id, municipality.Id,
            omitStreet: true);

        var response = await animals.PostCreateAsync("2026/0001", "t-1", "Burek", "Mix", "none", AnimalSpecies.Dog,
            AnimalSex.Male, extraFields: fields);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAnimal_ShouldStoreOrigin_AndReturnItWithMunicipalityNames()
    {
        var admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(_shelterId));
        await SetModuleAsync(admin, true);
        var found = await CreateMunicipalityAsync(admin, "Gmina Znalezienia");
        var contractor = await CreateMunicipalityAsync(admin, "Gmina Rozliczeniowa");
        var animals = new AnimalFactory(new ApiClient(admin));

        var id = await animals.CreateAsync("2026/0001", "t-1", "Burek", "Mix", "none", AnimalSpecies.Dog,
            AnimalSex.Male,
            extraFields: OriginFields(AdmissionMethod.Intervention, "Leśna 1", found.Id, contractor.Id));

        var animal = await animals.GetAsync(id);

        animal.Origin.Should().NotBeNull();
        animal.Origin!.IsAssigned.Should().BeTrue();
        animal.Origin.AdmissionDate.Should().Be(AdmissionDate);
        animal.Origin.AdmissionMethod.Should().Be(AdmissionMethod.Intervention);
        animal.Origin.FoundStreet.Should().Be("Leśna 1");
        animal.Origin.FoundMunicipality.Should().Be(new MunicipalityReferenceDto(found.Id, "Gmina Znalezienia"));
        animal.Origin.ContractorMunicipality.Should()
            .Be(new MunicipalityReferenceDto(contractor.Id, "Gmina Rozliczeniowa"));
    }

    [Fact]
    public async Task GetAnimal_ShouldReturnUnassignedOrigin_ForAnimalCreatedBeforeModuleWasEnabled()
    {
        var admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(_shelterId));
        var animals = new AnimalFactory(new ApiClient(admin));
        var id = await animals.CreateAsync("2026/0001", "t-1", "Burek", "Mix", "none", AnimalSpecies.Dog,
            AnimalSex.Male);

        (await animals.GetAsync(id)).Origin.Should().BeNull();

        await SetModuleAsync(admin, true);

        var origin = (await animals.GetAsync(id)).Origin;
        origin.Should().Be(AnimalOriginDto.Unassigned);
        origin!.ContractorMunicipality.Name.Should().Be("Brak przypisania");
    }

    [Fact]
    public async Task UpdateAnimal_ShouldAddAndThenChangeOrigin()
    {
        var admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(_shelterId));
        var animals = new AnimalFactory(new ApiClient(admin));
        var id = await animals.CreateAsync("2026/0001", "t-1", "Burek", "Mix", "none", AnimalSpecies.Dog,
            AnimalSex.Male);
        await SetModuleAsync(admin, true);
        var first = await CreateMunicipalityAsync(admin, "Gmina 1");
        var second = await CreateMunicipalityAsync(admin, "Gmina 2");

        var withoutOrigin = await animals.PutUpdateAsync(id, "2026/0001", "t-1", "Burek", "Mix", "none",
            AnimalSpecies.Dog, AnimalSex.Male, []);
        withoutOrigin.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await animals.UpdateAsync(id, "2026/0001", "t-1", "Burek", "Mix", "none", AnimalSpecies.Dog,
            AnimalSex.Male, [], extraFields: OriginFields(AdmissionMethod.Abandoned, "Polna 2", first.Id, first.Id));
        (await animals.GetAsync(id)).Origin!.FoundStreet.Should().Be("Polna 2");

        await animals.UpdateAsync(id, "2026/0001", "t-1", "Burek", "Mix", "none", AnimalSpecies.Dog,
            AnimalSex.Male, [],
            extraFields: OriginFields(AdmissionMethod.Other, "Długa 3", first.Id, second.Id));

        var origin = (await animals.GetAsync(id)).Origin!;
        origin.AdmissionMethod.Should().Be(AdmissionMethod.Other);
        origin.FoundStreet.Should().Be("Długa 3");
        origin.ContractorMunicipality.Id.Should().Be(second.Id);
    }

    [Fact]
    public async Task CreateAnimal_ShouldRejectArchivedMunicipality()
    {
        var admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(_shelterId));
        await SetModuleAsync(admin, true);
        var municipality = await CreateMunicipalityAsync(admin, "Gmina Archiwalna");
        await admin.DeleteAsync(SetMunicipalityArchivedRequest.BuildArchiveRoute(municipality.Id));
        var animals = new AnimalFactory(new ApiClient(admin));

        var response = await animals.PostCreateAsync("2026/0001", "t-1", "Burek", "Mix", "none", AnimalSpecies.Dog,
            AnimalSex.Male,
            extraFields: OriginFields(AdmissionMethod.Abandoned, "Leśna 1", municipality.Id, municipality.Id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DisablingModule_ShouldHideOrigin_AndKeepItForReEnabling()
    {
        var admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(_shelterId));
        await SetModuleAsync(admin, true);
        var municipality = await CreateMunicipalityAsync(admin, "Gmina A");
        var animals = new AnimalFactory(new ApiClient(admin));
        var id = await animals.CreateAsync("2026/0001", "t-1", "Burek", "Mix", "none", AnimalSpecies.Dog,
            AnimalSex.Male,
            extraFields: OriginFields(AdmissionMethod.Abandoned, "Leśna 1", municipality.Id, municipality.Id));

        await SetModuleAsync(admin, false);

        (await animals.GetAsync(id)).Origin.Should().BeNull();
        await animals.UpdateAsync(id, "2026/0001", "t-1", "Burek", "Mix", "none", AnimalSpecies.Dog,
            AnimalSex.Male, []);

        await SetModuleAsync(admin, true);

        (await animals.GetAsync(id)).Origin!.FoundStreet.Should().Be("Leśna 1");
    }

    [Theory]
    [InlineData("monthly?year=2026&month=3", "RaportPochodzeniaMiesieczny_03_2026.pdf")]
    [InlineData("yearly?year=2026", "ZestawienieRocznePochodzenia_2026.pdf")]
    public async Task OriginReports_ShouldReturnPdf_WhenModuleIsEnabled(string query, string fileName)
    {
        var admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(_shelterId));
        var animals = new AnimalFactory(new ApiClient(admin));
        await animals.CreateAsync("2026/0001", "t-1", "Stary", "Mix", "none", AnimalSpecies.Dog, AnimalSex.Male);
        await SetModuleAsync(admin, true);
        var municipality = await CreateMunicipalityAsync(admin, "Gmina A");
        await animals.CreateAsync("2026/0002", "t-2", "Nowy", "Mix", "none", AnimalSpecies.Cat, AnimalSex.Female,
            extraFields: OriginFields(AdmissionMethod.Abandoned, "Leśna 1", municipality.Id, municipality.Id));

        var response = await admin.GetAsync($"/reports/origin/{query}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        response.Content.Headers.GetValues("Content-Disposition").Single().Should().Contain(fileName);
        var pdf = await response.Content.ReadAsByteArrayAsync();
        pdf.Take(4).Should().Equal(0x25, 0x50, 0x44, 0x46);
    }

    [Fact]
    public async Task OriginMonthlyReport_ShouldValidateMonth()
    {
        var admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(_shelterId));
        await SetModuleAsync(admin, true);

        var response = await admin.GetAsync($"{GenerateOriginMonthlyReportRequest.Route}?year=2026&month=13");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExistingReports_ShouldStillWork_WhenModuleIsEnabled()
    {
        var admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(_shelterId));
        var animals = new AnimalFactory(new ApiClient(admin));
        await animals.CreateAsync("2026/0001", "t-1", "Stary", "Mix", "none", AnimalSpecies.Dog, AnimalSex.Male);
        await SetModuleAsync(admin, true);

        var response = await admin.GetAsync("/reports/animals/dump");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static Dictionary<string, string> OriginFields(AdmissionMethod method, string street, Guid foundId,
        Guid contractorId, bool omitStreet = false)
    {
        var origin = new Dictionary<string, object>
        {
            ["admissionDate"] = AdmissionDate,
            ["admissionMethod"] = (int)method,
            ["foundStreet"] = street,
            ["foundMunicipalityId"] = foundId,
            ["contractorMunicipalityId"] = contractorId,
        };
        if (omitStreet)
        {
            origin.Remove("foundStreet");
        }

        return new Dictionary<string, string> { ["Origin"] = JsonSerializer.Serialize(origin) };
    }

    private static async Task SetModuleAsync(HttpClient admin, bool isEnabled)
    {
        var response = await admin.PutAsJsonAsync(SetShelterFeatureStateRequest.BuildRoute(FeatureKey),
            new { IsEnabled = isEnabled });
        response.EnsureSuccessStatusCode();
    }

    private static async Task<MunicipalityDto> CreateMunicipalityAsync(HttpClient admin, string name)
    {
        var response = await admin.PostAsJsonAsync(CreateMunicipalityRequest.Route, new { Name = name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MunicipalityDto>())!;
    }
}
