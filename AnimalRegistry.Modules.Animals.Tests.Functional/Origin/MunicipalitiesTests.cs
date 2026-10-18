using AnimalRegistry.Modules.Animals.Api.Municipalities;
using AnimalRegistry.Modules.Animals.Application.Municipalities;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Origin;

[Collection("Sequential")]
public sealed class MunicipalitiesTests(ApiTestFixture fixture) : OriginModuleTestBase(fixture)
{
    [Fact]
    public async Task Endpoints_ShouldReturnForbidden_WhenModuleIsDisabled()
    {
        var id = Guid.NewGuid();

        var responses = new[]
        {
            await Admin.GetAsync(ListMunicipalitiesRequest.Route),
            await Admin.PostAsJsonAsync(CreateMunicipalityRequest.Route, new { Name = "Gmina" }),
            await Admin.PutAsJsonAsync(UpdateMunicipalityRequest.BuildRoute(id), new { Name = "Gmina" }),
            await Admin.DeleteAsync(ArchiveMunicipalityRequest.BuildRoute(id)),
            await Admin.PostAsync(RestoreMunicipalityRequest.BuildRoute(id), null),
        };

        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ManagementEndpoints_ShouldBeAvailableToShelterWorker()
    {
        await SetModuleAsync(true);

        var created = await CreateMunicipalityAsync("Gmina A", Worker);

        var responses = new[]
        {
            await Worker.PutAsJsonAsync(UpdateMunicipalityRequest.BuildRoute(created.Id), new { Name = "Gmina B" }),
            await Worker.DeleteAsync(ArchiveMunicipalityRequest.BuildRoute(created.Id)),
            await Worker.PostAsync(RestoreMunicipalityRequest.BuildRoute(created.Id), null),
        };

        responses.Should().OnlyContain(r => r.IsSuccessStatusCode);
        var municipalities = await Worker.GetFromJsonAsync<List<MunicipalityDto>>(ListMunicipalitiesRequest.Route);
        municipalities!.Should().ContainSingle(m => m.Id == created.Id && m.Name == "Gmina B" && !m.IsArchived);
    }

    [Fact]
    public async Task List_ShouldBeAvailableToShelterWorker_AndSortedByName()
    {
        await SetModuleAsync(true);
        await CreateMunicipalityAsync("Ząbki");
        await CreateMunicipalityAsync("Łomianki");
        await CreateMunicipalityAsync("Gmina A");

        var municipalities = await Worker.GetFromJsonAsync<List<MunicipalityDto>>(ListMunicipalitiesRequest.Route);

        municipalities!.Select(m => m.Name).Should().Equal("Gmina A", "Łomianki", "Ząbki");
    }

    [Fact]
    public async Task List_ShouldReturnOnlyMunicipalitiesOfOwnShelter()
    {
        await SetModuleAsync(true);
        await CreateMunicipalityAsync("Gmina A");
        var otherAdmin = await CreateOtherShelterAdminWithModuleAsync();
        await CreateMunicipalityAsync("Gmina Obca", otherAdmin);

        var municipalities = await Worker.GetFromJsonAsync<List<MunicipalityDto>>(ListMunicipalitiesRequest.Route);

        municipalities!.Select(m => m.Name).Should().Equal("Gmina A");
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedMunicipality_WithTrimmedName()
    {
        await SetModuleAsync(true);

        var municipality = await CreateMunicipalityAsync("  Gmina A ");

        municipality.Name.Should().Be("Gmina A");
        municipality.IsArchived.Should().BeFalse();
        municipality.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_ShouldReturnBadRequest_WhenNameIsEmpty(string name)
    {
        await SetModuleAsync(true);

        var response = await Admin.PostAsJsonAsync(CreateMunicipalityRequest.Route, new { Name = name });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenNameIsTooLong()
    {
        await SetModuleAsync(true);

        var response = await Admin.PostAsJsonAsync(CreateMunicipalityRequest.Route,
            new { Name = new string('a', 201) });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenNameAlreadyExists()
    {
        await SetModuleAsync(true);
        await CreateMunicipalityAsync("Gmina A");

        var response = await Admin.PostAsJsonAsync(CreateMunicipalityRequest.Route, new { Name = " Gmina A " });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Municipality 'Gmina A' already exists.");
    }

    [Fact]
    public async Task Create_ShouldAllowSameName_InAnotherShelter()
    {
        await SetModuleAsync(true);
        await CreateMunicipalityAsync("Gmina A");
        var otherAdmin = await CreateOtherShelterAdminWithModuleAsync();

        var response = await otherAdmin.PostAsJsonAsync(CreateMunicipalityRequest.Route, new { Name = "Gmina A" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_ShouldNotReturnServerError_WhenSameNameIsCreatedConcurrently()
    {
        await SetModuleAsync(true);

        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ =>
            Admin.PostAsJsonAsync(CreateMunicipalityRequest.Route, new { Name = "Gmina A" })));

        responses.Count(r => r.StatusCode == HttpStatusCode.OK).Should().Be(1);
        responses.Should().OnlyContain(r =>
            r.StatusCode == HttpStatusCode.OK || r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ShouldRenameMunicipality()
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");

        var response = await Admin.PutAsJsonAsync(UpdateMunicipalityRequest.BuildRoute(municipality.Id),
            new { Name = " Gmina B " });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<MunicipalityDto>())!.Name.Should().Be("Gmina B");
        var list = await Worker.GetFromJsonAsync<List<MunicipalityDto>>(ListMunicipalitiesRequest.Route);
        list!.Should().ContainSingle().Which.Name.Should().Be("Gmina B");
    }

    [Fact]
    public async Task Update_ShouldAllowKeepingOwnName()
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");

        var response = await Admin.PutAsJsonAsync(UpdateMunicipalityRequest.BuildRoute(municipality.Id),
            new { Name = "Gmina A" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenNameIsTakenByAnotherMunicipality()
    {
        await SetModuleAsync(true);
        await CreateMunicipalityAsync("Gmina A");
        var second = await CreateMunicipalityAsync("Gmina B");

        var response = await Admin.PutAsJsonAsync(UpdateMunicipalityRequest.BuildRoute(second.Id),
            new { Name = "Gmina A" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenNameIsEmpty()
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");

        var response = await Admin.PutAsJsonAsync(UpdateMunicipalityRequest.BuildRoute(municipality.Id),
            new { Name = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenMunicipalityDoesNotExist()
    {
        await SetModuleAsync(true);

        var response = await Admin.PutAsJsonAsync(UpdateMunicipalityRequest.BuildRoute(Guid.NewGuid()),
            new { Name = "Gmina A" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenMunicipalityBelongsToAnotherShelter()
    {
        var otherAdmin = await CreateOtherShelterAdminWithModuleAsync();
        var foreign = await CreateMunicipalityAsync("Gmina Obca", otherAdmin);
        await SetModuleAsync(true);

        var response = await Admin.PutAsJsonAsync(UpdateMunicipalityRequest.BuildRoute(foreign.Id),
            new { Name = "Gmina A" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Archive_ShouldHideMunicipalityFromDefaultList()
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");

        var response = await Admin.DeleteAsync(ArchiveMunicipalityRequest.BuildRoute(municipality.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<MunicipalityDto>())!.IsArchived.Should().BeTrue();
        var active = await Worker.GetFromJsonAsync<List<MunicipalityDto>>(ListMunicipalitiesRequest.Route);
        var all = await Worker.GetFromJsonAsync<List<MunicipalityDto>>(
            $"{ListMunicipalitiesRequest.Route}?includeArchived=true");
        active.Should().BeEmpty();
        all.Should().ContainSingle().Which.IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task Archive_ShouldReturnNotFound_WhenMunicipalityDoesNotExist()
    {
        await SetModuleAsync(true);

        var response = await Admin.DeleteAsync(ArchiveMunicipalityRequest.BuildRoute(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Restore_ShouldBringMunicipalityBackToDefaultList()
    {
        await SetModuleAsync(true);
        var municipality = await CreateArchivedMunicipalityAsync("Gmina A");

        var response = await Admin.PostAsync(RestoreMunicipalityRequest.BuildRoute(municipality.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<MunicipalityDto>())!.IsArchived.Should().BeFalse();
        var active = await Worker.GetFromJsonAsync<List<MunicipalityDto>>(ListMunicipalitiesRequest.Route);
        active.Should().ContainSingle().Which.Id.Should().Be(municipality.Id);
    }

    [Fact]
    public async Task Restore_ShouldReturnNotFound_WhenMunicipalityDoesNotExist()
    {
        await SetModuleAsync(true);

        var response = await Admin.PostAsync(RestoreMunicipalityRequest.BuildRoute(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
