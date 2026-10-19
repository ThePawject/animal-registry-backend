using AnimalRegistry.Modules.Animals.Api.Vaccinations;
using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Modules.Shelters.Api;
using AnimalRegistry.Shared.Testing;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Vaccinations;

public abstract class VaccinationModuleTestBase : IntegrationTestBase
{
    protected const string FeatureKey = "animal-vaccinations";

    protected readonly string ShelterId = $"test-shelter-vacc-{Guid.NewGuid():N}";
    protected readonly string OtherShelterId = $"test-shelter-other-{Guid.NewGuid():N}";
    protected readonly HttpClient Admin;
    protected readonly HttpClient Worker;
    protected readonly AnimalFactory Animals;

    protected VaccinationModuleTestBase(ApiTestFixture fixture) : base(fixture)
    {
        Admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(ShelterId));
        Worker = Factory.CreateAuthenticatedClient(TestUser.WithShelterAccess(ShelterId));
        Animals = new AnimalFactory(new ApiClient(Admin));
    }

    protected static DateTimeOffset LocalDay(int daysFromToday)
    {
        var today = ShelterCalendar.ToLocal(DateTimeOffset.UtcNow).Date;
        return ShelterCalendar.StartOfDay(today.AddDays(daysFromToday));
    }

    protected static Task<HttpResponseMessage> PutModuleAsync(HttpClient client, bool isEnabled)
    {
        return client.PutAsJsonAsync(SetShelterFeatureStateRequest.BuildRoute(FeatureKey),
            new { IsEnabled = isEnabled });
    }

    protected async Task SetModuleAsync(bool isEnabled)
    {
        (await PutModuleAsync(Admin, isEnabled)).EnsureSuccessStatusCode();
    }

    protected async Task<HttpClient> CreateOtherShelterAdminWithModuleAsync()
    {
        var client = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(OtherShelterId));
        (await PutModuleAsync(client, true)).EnsureSuccessStatusCode();
        return client;
    }

    protected Task<Guid> CreateAnimalAsync(string signature, AnimalFactory? factory = null)
    {
        return (factory ?? Animals).CreateAsync(signature, $"t-{signature}", "Burek", "Mix", "none",
            AnimalSpecies.Dog, AnimalSex.Male);
    }

    protected static object Body(
        string vaccineName = "Nobivac Rabies",
        DateTimeOffset? administeredOn = null,
        DateTimeOffset? validUntil = null,
        string? veterinarian = null,
        string? batchNumber = null,
        string? notes = null,
        Guid? replacesVaccinationId = null)
    {
        return new
        {
            VaccineName = vaccineName,
            AdministeredOn = administeredOn ?? LocalDay(-1),
            ValidUntil = validUntil,
            Veterinarian = veterinarian,
            BatchNumber = batchNumber,
            Notes = notes,
            ReplacesVaccinationId = replacesVaccinationId,
        };
    }

    protected static Task<HttpResponseMessage> PostVaccinationAsync(HttpClient client, Guid animalId, object body)
    {
        return client.PostAsJsonAsync(CreateAnimalVaccinationRequest.BuildRoute(animalId), body);
    }

    protected async Task<AnimalVaccinationDto> CreateVaccinationAsync(Guid animalId, object? body = null,
        HttpClient? client = null)
    {
        var response = await PostVaccinationAsync(client ?? Worker, animalId, body ?? Body());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AnimalVaccinationDto>())!;
    }

    protected async Task<List<AnimalVaccinationDto>> ListAsync(Guid animalId)
    {
        return (await Worker.GetFromJsonAsync<List<AnimalVaccinationDto>>(
            ListAnimalVaccinationsRequest.BuildRoute(animalId)))!;
    }

    protected Task<HttpResponseMessage> UploadAttachmentAsync(Guid animalId, Guid vaccinationId,
        string fileName = "scan.pdf", string contentType = "application/pdf")
    {
        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent("%PDF-1.4 test"u8.ToArray());
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(file, "File", fileName);
        return Worker.PutAsync(UploadVaccinationAttachmentRequest.BuildRoute(animalId, vaccinationId), content);
    }
}
