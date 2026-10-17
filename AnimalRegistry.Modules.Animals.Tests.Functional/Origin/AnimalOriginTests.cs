using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Origin;

[Collection("Sequential")]
public sealed class AnimalOriginTests(ApiTestFixture fixture) : OriginModuleTestBase(fixture)
{
    private Task<bool> OriginExistsAsync(Guid animalId)
    {
        return WithDbAsync(db => db.AnimalOrigins.AnyAsync(o => o.AnimalId == animalId));
    }

    private Task<int> AnimalCountAsync()
    {
        return WithDbAsync(db => db.Animals.CountAsync(a => a.ShelterId == ShelterId));
    }

    private static async Task ShouldBeBadRequestAsync(HttpResponseMessage response, string message)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain(message);
    }

    private static async Task ShouldBeConflictAsync(HttpResponseMessage response, string message)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync()).Should().Contain(message);
    }

    [Fact]
    public async Task CreateAnimal_ShouldNotRequireOrigin_WhenModuleIsDisabled()
    {
        var id = await CreateAnimalAsync("2026/0001");

        (await Animals.GetAsync(id)).Origin.Should().BeNull();
        (await OriginExistsAsync(id)).Should().BeFalse();
    }

    [Fact]
    public async Task CreateAnimal_ShouldIgnoreOrigin_WhenModuleIsDisabled()
    {
        var id = await CreateAnimalAsync("2026/0001", OriginFields(Guid.NewGuid(), Guid.NewGuid()));

        (await OriginExistsAsync(id)).Should().BeFalse();
        await SetModuleAsync(true);
        (await Animals.GetAsync(id)).Origin.Should().Be(AnimalOriginDto.Unassigned);
    }

    [Fact]
    public async Task CreateAnimal_ShouldStoreOrigin_AndReturnItWithMunicipalityNames()
    {
        await SetModuleAsync(true);
        var found = await CreateMunicipalityAsync("Gmina Znalezienia");
        var contractor = await CreateMunicipalityAsync("Gmina Rozliczeniowa");

        var id = await CreateAnimalAsync("2026/0001",
            OriginFields(found.Id, contractor.Id, AdmissionMethod.Intervention, "  Leśna 1 "));

        var origin = (await Animals.GetAsync(id)).Origin!;
        origin.IsAssigned.Should().BeTrue();
        origin.AdmissionDate.Should().Be(AdmissionDate);
        origin.AdmissionMethod.Should().Be(AdmissionMethod.Intervention);
        origin.AdmissionMethodName.Should().Be("Interwencja (Straż Miejska / Policja)");
        origin.FoundStreet.Should().Be("Leśna 1");
        origin.FoundMunicipality.Should().Be(new MunicipalityReferenceDto(found.Id, "Gmina Znalezienia"));
        origin.ContractorMunicipality.Should().Be(new MunicipalityReferenceDto(contractor.Id, "Gmina Rozliczeniowa"));
        (await OriginExistsAsync(id)).Should().BeTrue();
    }

    [Fact]
    public async Task CreateAnimal_ShouldReturnBadRequest_WhenOriginIsMissing()
    {
        await SetModuleAsync(true);

        var response = await PostAnimalAsync("2026/0001");

        await ShouldBeBadRequestAsync(response, "Animal origin is required when the origin module is enabled.");
    }

    [Fact]
    public async Task CreateAnimal_ShouldReturnBadRequest_WhenOriginIsNotValidJson()
    {
        await SetModuleAsync(true);

        var response = await PostAnimalAsync("2026/0001", new Dictionary<string, string> { ["Origin"] = "{not json" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public static TheoryData<string> MissingOriginFields => new()
    {
        "admissionDate", "admissionMethod", "foundStreet", "foundMunicipalityId", "contractorMunicipalityId",
    };

    [Theory]
    [MemberData(nameof(MissingOriginFields))]
    public async Task CreateAnimal_ShouldReturnBadRequest_WhenOriginFieldIsMissing(string field)
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");
        var origin = JsonSerializer.Deserialize<Dictionary<string, object?>>(
            OriginFields(municipality.Id, municipality.Id)["Origin"])!;
        origin.Remove(field);

        var response = await PostAnimalAsync("2026/0001",
            new Dictionary<string, string> { ["Origin"] = JsonSerializer.Serialize(origin) });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await AnimalCountAsync()).Should().Be(0);
    }

    [Theory]
    [InlineData(AdmissionMethod.Unassigned)]
    [InlineData((AdmissionMethod)999)]
    public async Task CreateAnimal_ShouldReturnBadRequest_WhenAdmissionMethodIsInvalid(AdmissionMethod method)
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");

        var response = await PostAnimalAsync("2026/0001", OriginFields(municipality.Id, municipality.Id, method));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAnimal_ShouldReturnBadRequest_WhenStreetIsBlank()
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");

        var response = await PostAnimalAsync("2026/0001",
            OriginFields(municipality.Id, municipality.Id, street: "   "));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAnimal_ShouldReturnBadRequest_WhenStreetIsTooLong()
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");

        var response = await PostAnimalAsync("2026/0001",
            OriginFields(municipality.Id, municipality.Id, street: new string('a', 201)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAnimal_ShouldReturnConflict_WhenAdmissionDateIsInFuture()
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");

        var response = await PostAnimalAsync("2026/0001",
            OriginFields(municipality.Id, municipality.Id, admissionDate: DateTimeOffset.UtcNow.AddDays(2)));

        await ShouldBeConflictAsync(response, "Admission date cannot be in the future.");
        (await AnimalCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateAnimal_ShouldAcceptAdmissionDateToday()
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");

        var response = await PostAnimalAsync("2026/0001",
            OriginFields(municipality.Id, municipality.Id, admissionDate: DateTimeOffset.UtcNow));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateAnimal_ShouldReturnConflict_WhenMunicipalityDoesNotExist()
    {
        await SetModuleAsync(true);
        var found = await CreateMunicipalityAsync("Gmina A");
        var unknownId = Guid.NewGuid();

        var response = await PostAnimalAsync("2026/0001", OriginFields(found.Id, unknownId));

        await ShouldBeConflictAsync(response, $"The contractor municipality '{unknownId}' does not exist.");
        (await AnimalCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateAnimal_ShouldReturnConflict_WhenMunicipalityBelongsToAnotherShelter()
    {
        var otherAdmin = await CreateOtherShelterAdminWithModuleAsync();
        var foreign = await CreateMunicipalityAsync("Gmina Obca", otherAdmin);
        await SetModuleAsync(true);
        var own = await CreateMunicipalityAsync("Gmina A");

        var response = await PostAnimalAsync("2026/0001", OriginFields(foreign.Id, own.Id));

        await ShouldBeConflictAsync(response, $"The found municipality '{foreign.Id}' does not exist.");
    }

    [Fact]
    public async Task CreateAnimal_ShouldReturnConflict_WhenMunicipalityIsArchived()
    {
        await SetModuleAsync(true);
        var archived = await CreateArchivedMunicipalityAsync("Gmina Archiwalna");

        var response = await PostAnimalAsync("2026/0001", OriginFields(archived.Id, archived.Id));

        await ShouldBeConflictAsync(response, "The found municipality 'Gmina Archiwalna' is archived.");
        (await AnimalCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task GetAnimal_ShouldNotReturnOrigin_WhenModuleIsDisabled()
    {
        var id = await CreateAnimalAsync("2026/0001");

        (await Animals.GetAsync(id)).Origin.Should().BeNull();
    }

    [Fact]
    public async Task GetAnimal_ShouldReturnUnassignedOrigin_ForAnimalCreatedBeforeModuleWasEnabled()
    {
        var id = await CreateAnimalAsync("2026/0001");

        await SetModuleAsync(true);

        var origin = (await Animals.GetAsync(id)).Origin;
        origin.Should().Be(AnimalOriginDto.Unassigned);
        origin!.ContractorMunicipality.Name.Should().Be("Brak przypisania");
    }

    [Fact]
    public async Task GetAnimal_ShouldReturnNameOfArchivedMunicipality()
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");
        var id = await CreateAnimalAsync("2026/0001", OriginFields(municipality.Id, municipality.Id));

        await ArchiveMunicipalityAsync(municipality.Id);

        (await Animals.GetAsync(id)).Origin!.FoundMunicipality.Name.Should().Be("Gmina A");
    }

    [Fact]
    public async Task UpdateAnimal_ShouldReturnBadRequest_WhenOriginIsMissing()
    {
        var id = await CreateAnimalAsync("2026/0001");
        await SetModuleAsync(true);

        var response = await PutAnimalAsync(id, "2026/0001", name: "Azor");

        await ShouldBeBadRequestAsync(response, "Animal origin is required when the origin module is enabled.");
        (await Animals.GetAsync(id)).Name.Should().Be("Burek");
    }

    [Fact]
    public async Task UpdateAnimal_ShouldAddOrigin_ToAnimalCreatedBeforeModuleWasEnabled()
    {
        var id = await CreateAnimalAsync("2026/0001");
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");

        var response = await PutAnimalAsync(id, "2026/0001", OriginFields(municipality.Id, municipality.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var origin = (await Animals.GetAsync(id)).Origin!;
        origin.IsAssigned.Should().BeTrue();
        origin.FoundStreet.Should().Be("Leśna 1");
    }

    [Fact]
    public async Task UpdateAnimal_ShouldChangeExistingOrigin_WithoutCreatingAnotherOne()
    {
        await SetModuleAsync(true);
        var first = await CreateMunicipalityAsync("Gmina 1");
        var second = await CreateMunicipalityAsync("Gmina 2");
        var id = await CreateAnimalAsync("2026/0001", OriginFields(first.Id, first.Id));
        var date = AdmissionDate.AddDays(-10);

        var response = await PutAnimalAsync(id, "2026/0001",
            OriginFields(second.Id, second.Id, AdmissionMethod.Other, "Długa 3", date));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var origin = (await Animals.GetAsync(id)).Origin!;
        origin.AdmissionDate.Should().Be(date);
        origin.AdmissionMethod.Should().Be(AdmissionMethod.Other);
        origin.FoundStreet.Should().Be("Długa 3");
        origin.FoundMunicipality.Id.Should().Be(second.Id);
        origin.ContractorMunicipality.Id.Should().Be(second.Id);
        (await WithDbAsync(db => db.AnimalOrigins.CountAsync(o => o.AnimalId == id))).Should().Be(1);
    }

    [Fact]
    public async Task UpdateAnimal_ShouldKeepAnimalAndOriginUnchanged_WhenOriginIsInvalid()
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");
        var id = await CreateAnimalAsync("2026/0001", OriginFields(municipality.Id, municipality.Id));

        var response = await PutAnimalAsync(id, "2026/0001",
            OriginFields(municipality.Id, Guid.NewGuid(), street: "Polna 2"), name: "Azor");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var animal = await Animals.GetAsync(id);
        animal.Name.Should().Be("Burek");
        animal.Origin!.FoundStreet.Should().Be("Leśna 1");
    }

    [Fact]
    public async Task UpdateAnimal_ShouldStoreSingleOrigin_WhenOriginIsAddedConcurrently()
    {
        var id = await CreateAnimalAsync("2026/0001");
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");

        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ =>
            PutAnimalAsync(id, "2026/0001", OriginFields(municipality.Id, municipality.Id))));

        responses.Should().Contain(r => r.StatusCode == HttpStatusCode.OK);
        responses.Should().OnlyContain(r =>
            r.StatusCode == HttpStatusCode.OK || r.StatusCode == HttpStatusCode.BadRequest);
        (await WithDbAsync(db => db.AnimalOrigins.CountAsync(o => o.AnimalId == id))).Should().Be(1);
    }

    [Fact]
    public async Task UpdateAnimal_ShouldAcceptArchivedMunicipality_AlreadyAssignedToAnimal()
    {
        await SetModuleAsync(true);
        var found = await CreateMunicipalityAsync("Gmina A");
        var contractor = await CreateMunicipalityAsync("Gmina B");
        var id = await CreateAnimalAsync("2026/0001", OriginFields(found.Id, contractor.Id));
        await ArchiveMunicipalityAsync(found.Id);

        var sameRole = await PutAnimalAsync(id, "2026/0001", OriginFields(found.Id, contractor.Id, street: "Polna 2"));
        var otherRole = await PutAnimalAsync(id, "2026/0001", OriginFields(found.Id, found.Id));

        sameRole.StatusCode.Should().Be(HttpStatusCode.OK);
        otherRole.StatusCode.Should().Be(HttpStatusCode.OK);
        (await Animals.GetAsync(id)).Origin!.ContractorMunicipality.Id.Should().Be(found.Id);
    }

    [Fact]
    public async Task UpdateAnimal_ShouldReturnConflict_WhenArchivedMunicipalityIsNewlyAssigned()
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");
        var archived = await CreateArchivedMunicipalityAsync("Gmina Archiwalna");
        var id = await CreateAnimalAsync("2026/0001", OriginFields(municipality.Id, municipality.Id));

        var response = await PutAnimalAsync(id, "2026/0001", OriginFields(municipality.Id, archived.Id));

        await ShouldBeConflictAsync(response, "The contractor municipality 'Gmina Archiwalna' is archived.");
    }

    [Fact]
    public async Task UpdateAnimal_ShouldIgnoreOriginAndKeepStoredData_WhenModuleIsDisabled()
    {
        await SetModuleAsync(true);
        var first = await CreateMunicipalityAsync("Gmina 1");
        var second = await CreateMunicipalityAsync("Gmina 2");
        var id = await CreateAnimalAsync("2026/0001", OriginFields(first.Id, first.Id));
        await SetModuleAsync(false);

        var withoutOrigin = await PutAnimalAsync(id, "2026/0001");
        var withOrigin = await PutAnimalAsync(id, "2026/0001", OriginFields(second.Id, second.Id, street: "Polna 2"));

        withoutOrigin.StatusCode.Should().Be(HttpStatusCode.OK);
        withOrigin.StatusCode.Should().Be(HttpStatusCode.OK);
        (await Animals.GetAsync(id)).Origin.Should().BeNull();
        await SetModuleAsync(true);
        var origin = (await Animals.GetAsync(id)).Origin!;
        origin.FoundStreet.Should().Be("Leśna 1");
        origin.FoundMunicipality.Id.Should().Be(first.Id);
    }

    [Fact]
    public async Task DeleteAnimal_ShouldRemoveItsOrigin()
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");
        var id = await CreateAnimalAsync("2026/0001", OriginFields(municipality.Id, municipality.Id));

        var response = await Admin.DeleteAsync(DeleteAnimalRequest.BuildRoute(id));

        response.IsSuccessStatusCode.Should().BeTrue();
        (await OriginExistsAsync(id)).Should().BeFalse();
    }
}
