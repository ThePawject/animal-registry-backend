using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Modules.Animals.Api.Vaccinations;
using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Modules.Shelters.Api;
using AnimalRegistry.Modules.Shelters.Application;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Vaccinations;

[Collection("Sequential")]
public sealed class AnimalVaccinationsTests(ApiTestFixture fixture) : VaccinationModuleTestBase(fixture)
{
    [Fact]
    public async Task GetShelterFeatures_ShouldListVaccinationModuleAsDisabled_ByDefault()
    {
        var features = await Worker.GetFromJsonAsync<List<ShelterFeatureDto>>(GetShelterFeatures.Route);

        var feature = features.Should().ContainSingle(f => f.Key == FeatureKey).Which;
        feature.IsEnabled.Should().BeFalse();
        feature.Name.Should().Be("Szczepienia");
    }

    [Fact]
    public async Task Endpoints_ShouldReturnForbidden_WhenModuleIsDisabled()
    {
        var animalId = await CreateAnimalAsync("2026/0001");
        var id = Guid.NewGuid();

        var responses = new[]
        {
            await Worker.GetAsync(ListAnimalVaccinationsRequest.BuildRoute(animalId)),
            await PostVaccinationAsync(Worker, animalId, Body()),
            await Worker.PutAsJsonAsync(UpdateAnimalVaccinationRequest.BuildRoute(animalId, id), Body()),
            await Worker.DeleteAsync(DeleteAnimalVaccinationRequest.BuildRoute(animalId, id)),
            await UploadAttachmentAsync(animalId, id),
            await Worker.DeleteAsync(DeleteVaccinationAttachmentRequest.BuildRoute(animalId, id)),
            await Worker.GetAsync(ListExpiringVaccinationsRequest.Route),
        };

        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_ShouldStoreVaccination_WithOnlyRequiredFields()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");
        var administeredOn = LocalDay(-3);

        var vaccination = await CreateVaccinationAsync(animalId, Body("  Biocan DHPPi ", administeredOn));

        vaccination.AnimalId.Should().Be(animalId);
        vaccination.VaccineName.Should().Be("Biocan DHPPi");
        vaccination.AdministeredOn.Should().Be(administeredOn);
        vaccination.ValidUntil.Should().BeNull();
        vaccination.Veterinarian.Should().BeNull();
        vaccination.BatchNumber.Should().BeNull();
        vaccination.Notes.Should().BeNull();
        vaccination.Attachment.Should().BeNull();
        vaccination.Status.Should().Be(VaccinationStatus.Active);
        vaccination.DaysUntilExpiry.Should().BeNull();
        vaccination.CreatedBy.Should().Be(TestUser.Email);
    }

    [Fact]
    public async Task List_ShouldReturnVaccinationsNewestFirst_WithActiveFlag()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");
        await CreateVaccinationAsync(animalId, Body("Old rabies", LocalDay(-400), LocalDay(-35), "Jan Kowalski", "S1"));
        await CreateVaccinationAsync(animalId, Body("New rabies", LocalDay(-30), LocalDay(335), "Anna Nowak", "S2",
            "Druga dawka"));

        var vaccinations = await ListAsync(animalId);

        vaccinations.Select(v => v.VaccineName).Should().Equal("New rabies", "Old rabies");
        vaccinations[0].Status.Should().Be(VaccinationStatus.Active);
        vaccinations[0].DaysUntilExpiry.Should().Be(335);
        vaccinations[0].Veterinarian.Should().Be("Anna Nowak");
        vaccinations[0].BatchNumber.Should().Be("S2");
        vaccinations[0].Notes.Should().Be("Druga dawka");
        vaccinations[1].Status.Should().Be(VaccinationStatus.Expired);
        vaccinations[1].DaysUntilExpiry.Should().Be(-35);
    }

    [Fact]
    public async Task Create_ShouldTreatValidityEndingToday_AsActive()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");

        var vaccination = await CreateVaccinationAsync(animalId, Body(validUntil: LocalDay(0)));

        vaccination.Status.Should().Be(VaccinationStatus.Active);
        vaccination.DaysUntilExpiry.Should().Be(0);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenNameOrDateIsMissing()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");

        var withoutName = await PostVaccinationAsync(Worker, animalId, Body(""));
        var withoutDate = await PostVaccinationAsync(Worker, animalId, new { VaccineName = "Rabies" });
        var tooLongNotes = await PostVaccinationAsync(Worker, animalId, Body(notes: new string('x', 1001)));

        withoutName.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        withoutDate.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tooLongNotes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenDatesBreakRules()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");

        var future = await PostVaccinationAsync(Worker, animalId, Body(administeredOn: LocalDay(1)));
        var validBeforeAdministration = await PostVaccinationAsync(Worker, animalId,
            Body(administeredOn: LocalDay(-1), validUntil: LocalDay(-2)));

        future.StatusCode.Should().Be(HttpStatusCode.Conflict);
        validBeforeAdministration.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await ListAsync(animalId)).Should().BeEmpty();
    }

    [Fact]
    public async Task Endpoints_ShouldReturnNotFound_ForAnimalOfAnotherShelter()
    {
        await SetModuleAsync(true);
        var otherAdmin = await CreateOtherShelterAdminWithModuleAsync();
        var otherAnimalId = await CreateAnimalAsync("2026/0001", new AnimalFactory(new ApiClient(otherAdmin)));
        var otherVaccination = await CreateVaccinationAsync(otherAnimalId, client: otherAdmin);

        var responses = new[]
        {
            await Worker.GetAsync(ListAnimalVaccinationsRequest.BuildRoute(otherAnimalId)),
            await PostVaccinationAsync(Worker, otherAnimalId, Body()),
            await Worker.PutAsJsonAsync(
                UpdateAnimalVaccinationRequest.BuildRoute(otherAnimalId, otherVaccination.Id), Body()),
            await Worker.DeleteAsync(DeleteAnimalVaccinationRequest.BuildRoute(otherAnimalId, otherVaccination.Id)),
            await UploadAttachmentAsync(otherAnimalId, otherVaccination.Id),
            await Worker.DeleteAsync(
                DeleteVaccinationAttachmentRequest.BuildRoute(otherAnimalId, otherVaccination.Id)),
        };

        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ShouldReplaceData()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");
        var created = await CreateVaccinationAsync(animalId, Body(veterinarian: "Jan", batchNumber: "S1"));

        var response = await Worker.PutAsJsonAsync(UpdateAnimalVaccinationRequest.BuildRoute(animalId, created.Id),
            Body("Rabisin", LocalDay(-2), LocalDay(10), notes: "Korekta"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await ListAsync(animalId)).Single();
        updated.Id.Should().Be(created.Id);
        updated.VaccineName.Should().Be("Rabisin");
        updated.AdministeredOn.Should().Be(LocalDay(-2));
        updated.ValidUntil.Should().Be(LocalDay(10));
        updated.Veterinarian.Should().BeNull();
        updated.BatchNumber.Should().BeNull();
        updated.Notes.Should().Be("Korekta");
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenVaccinationBelongsToAnotherAnimal()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");
        var otherAnimalId = await CreateAnimalAsync("2026/0002");
        var created = await CreateVaccinationAsync(animalId);

        var response = await Worker.PutAsJsonAsync(
            UpdateAnimalVaccinationRequest.BuildRoute(otherAnimalId, created.Id), Body("Rabisin"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldRemoveVaccination()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");
        var created = await CreateVaccinationAsync(animalId);
        (await UploadAttachmentAsync(animalId, created.Id)).EnsureSuccessStatusCode();

        var response = await Worker.DeleteAsync(DeleteAnimalVaccinationRequest.BuildRoute(animalId, created.Id));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ListAsync(animalId)).Should().BeEmpty();
    }

    [Fact]
    public async Task Attachment_ShouldBeUploadedReplacedAndDeleted()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");
        var created = await CreateVaccinationAsync(animalId);

        var first = await UploadAttachmentAsync(animalId, created.Id, "first.pdf");
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstDto = await first.Content.ReadFromJsonAsync<AnimalVaccinationDto>();
        firstDto!.Attachment!.FileName.Should().Be("first.pdf");
        firstDto.Attachment.ContentType.Should().Be("application/pdf");
        firstDto.Attachment.Url.Should().NotBeNullOrWhiteSpace();

        (await UploadAttachmentAsync(animalId, created.Id, "second.png", "image/png")).EnsureSuccessStatusCode();
        (await ListAsync(animalId)).Single().Attachment!.FileName.Should().Be("second.png");

        var deleted = await Worker.DeleteAsync(DeleteVaccinationAttachmentRequest.BuildRoute(animalId, created.Id));
        deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ListAsync(animalId)).Single().Attachment.Should().BeNull();

        var deletedAgain =
            await Worker.DeleteAsync(DeleteVaccinationAttachmentRequest.BuildRoute(animalId, created.Id));
        deletedAgain.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Attachment_ShouldReturnBadRequest_WhenFileIsMissingOrInvalid()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");
        var created = await CreateVaccinationAsync(animalId);

        var unsupported = await UploadAttachmentAsync(animalId, created.Id, "script.exe", "application/x-msdownload");
        var tooLongName = await UploadAttachmentAsync(animalId, created.Id, new string('a', 300) + ".pdf");
        var withoutContentType = new MultipartFormDataContent
        {
            { new StreamContent(new MemoryStream("%PDF-1.4"u8.ToArray())), "File", "scan.pdf" },
        };
        var noContentType = await Worker.PutAsync(
            UploadVaccinationAttachmentRequest.BuildRoute(animalId, created.Id), withoutContentType);
        var missing = await Worker.PutAsync(UploadVaccinationAttachmentRequest.BuildRoute(animalId, created.Id),
            new MultipartFormDataContent { { new StringContent("x"), "Other" } });

        unsupported.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tooLongName.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        noContentType.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        missing.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ListAsync(animalId)).Single().Attachment.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAnimal_ShouldRemoveItsVaccinations()
    {
        await SetModuleAsync(true);
        var animalId = await CreateAnimalAsync("2026/0001");
        await CreateVaccinationAsync(animalId, Body(validUntil: LocalDay(5)));

        (await Admin.DeleteAsync(DeleteAnimalRequest.BuildRoute(animalId))).EnsureSuccessStatusCode();

        var expiring = await Worker.GetFromJsonAsync<List<VaccinationWithAnimalDto>>(
            $"{ListExpiringVaccinationsRequest.Route}?expiresInDays=30");
        expiring.Should().BeEmpty();
    }
}
