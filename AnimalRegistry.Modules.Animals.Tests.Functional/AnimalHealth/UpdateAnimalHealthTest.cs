using AnimalRegistry.Modules.Animals.Api.AnimalHealth;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using JetBrains.Annotations;
using System.Net;
using System.Text;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.AnimalHealth;

[TestSubject(typeof(UpdateAnimalHealth))]
[Collection("Sequential")]
public class UpdateAnimalHealthTest(ApiTestFixture fixture) : IntegrationTestBase(fixture)
{
    private const string TestShelterId = "test-shelter-1";

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = Factory.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    private async Task AddHealthAsync(HttpClient client, Guid animalId, CreateAnimalHealthRequest request, byte[]? fileBytes = null, string? fileName = null)
    {
        using var multiPartContent = new MultipartFormDataContent();
        multiPartContent.Add(new StringContent(animalId.ToString()), "AnimalId");
        multiPartContent.Add(new StringContent(request.OccurredOn.ToString("o")), "OccurredOn");
        multiPartContent.Add(new StringContent(request.Description), "Description");

        if (fileBytes != null && fileName != null)
        {
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            multiPartContent.Add(fileContent, "DocumentFile", fileName);
        }

        var response = await client.PostAsync(CreateAnimalHealthRequest.BuildRoute(animalId), multiPartContent);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ShouldUpdateHealthRecord_WhenRequestIsValid()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);
        var client = Factory.CreateAuthenticatedClient(user);

        var animalId = await factory.CreateAsync(
            "2024/9104",
            "TRANS-HEALTH-3",
            "Health Update Animal",
            AnimalSpecies.Dog,
            AnimalSex.Female);

        var addRequest = new CreateAnimalHealthRequest
        {
            AnimalId = animalId, OccurredOn = DateTimeOffset.UtcNow.AddDays(-1), Description = "Original health",
        };
        await AddHealthAsync(client, animalId, addRequest);

        var animal = await factory.GetAsync(animalId);
        var recordId = animal.HealthRecords.First().Id;

        using var multiPartContent = new MultipartFormDataContent();
        multiPartContent.Add(new StringContent(animalId.ToString()), "AnimalId");
        multiPartContent.Add(new StringContent(recordId.ToString()), "HealthRecordId");
        multiPartContent.Add(new StringContent(DateTimeOffset.UtcNow.ToString("o")), "OccurredOn");
        multiPartContent.Add(new StringContent("Updated health"), "Description");

        var response = await client.PutAsync(
            UpdateAnimalHealthRequest.BuildRoute(animalId, recordId),
            multiPartContent);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var updatedAnimal = await factory.GetAsync(animalId);
        var updatedRecord = updatedAnimal.HealthRecords.First(r => r.Id == recordId);

        updatedRecord.Description.Should().Be("Updated health");
        updatedRecord.OccurredOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public async Task ShouldDeleteDocument_WhenDeleteDocumentIsTrue()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);
        var client = Factory.CreateAuthenticatedClient(user);

        var animalId = await factory.CreateAsync(
            "2024/9107",
            "TRANS-HEALTH-7",
            "Health Delete Doc",
            AnimalSpecies.Dog,
            AnimalSex.Male);

        var addRequest = new CreateAnimalHealthRequest
        {
            AnimalId = animalId,
            OccurredOn = DateTimeOffset.UtcNow.AddDays(-1),
            Description = "Original health"
        };
        await AddHealthAsync(client, animalId, addRequest);

        var animal = await factory.GetAsync(animalId);
        var recordId = animal.HealthRecords.First().Id;

        using var multiPartContent = new MultipartFormDataContent();
        multiPartContent.Add(new StringContent(animalId.ToString()), "AnimalId");
        multiPartContent.Add(new StringContent(recordId.ToString()), "HealthRecordId");
        multiPartContent.Add(new StringContent(DateTimeOffset.UtcNow.ToString("o")), "OccurredOn");
        multiPartContent.Add(new StringContent("Document deleted"), "Description");
        multiPartContent.Add(new StringContent("true"), "DeleteDocument");

        var response = await client.PutAsync(
            UpdateAnimalHealthRequest.BuildRoute(animalId, recordId),
            multiPartContent);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var updatedAnimal = await factory.GetAsync(animalId);
        var updatedRecord = updatedAnimal.HealthRecords.First(r => r.Id == recordId);

        updatedRecord.Document.Should().BeNull();
    }

    [Fact]
    public async Task ShouldKeepDocument_WhenFileIsNullAndDeleteIsFalse()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);
        var client = Factory.CreateAuthenticatedClient(user);

        var animalId = await factory.CreateAsync(
            "2024/9108",
            "TRANS-HEALTH-8",
            "Health Keep Doc",
            AnimalSpecies.Cat,
            AnimalSex.Female);

        var addRequest = new CreateAnimalHealthRequest
        {
            AnimalId = animalId,
            OccurredOn = DateTimeOffset.UtcNow.AddDays(-1),
            Description = "Original health"
        };
        await AddHealthAsync(client, animalId, addRequest);

        var animal = await factory.GetAsync(animalId);
        var recordId = animal.HealthRecords.First().Id;

        using var multiPartContent = new MultipartFormDataContent();
        multiPartContent.Add(new StringContent(animalId.ToString()), "AnimalId");
        multiPartContent.Add(new StringContent(recordId.ToString()), "HealthRecordId");
        multiPartContent.Add(new StringContent(DateTimeOffset.UtcNow.ToString("o")), "OccurredOn");
        multiPartContent.Add(new StringContent("No changes to document"), "Description");

        var response = await client.PutAsync(
            UpdateAnimalHealthRequest.BuildRoute(animalId, recordId),
            multiPartContent);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var updatedAnimal = await factory.GetAsync(animalId);
        var updatedRecord = updatedAnimal.HealthRecords.First(r => r.Id == recordId);

        updatedRecord.Document.Should().BeNull();
    }

    [Fact]
    public async Task ShouldKeepExistingDocument_WhenFileIsNullAndDeleteIsFalse()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);
        var client = Factory.CreateAuthenticatedClient(user);

        var animalId = await factory.CreateAsync(
            "2024/9109",
            "TRANS-HEALTH-9",
            "Health Keep Existing Doc",
            AnimalSpecies.Dog,
            AnimalSex.Male);

        var fileBytes = Encoding.UTF8.GetBytes("Original document content");
        var addRequest = new CreateAnimalHealthRequest
        {
            AnimalId = animalId,
            OccurredOn = DateTimeOffset.UtcNow.AddDays(-1),
            Description = "Health with document"
        };
        await AddHealthAsync(client, animalId, addRequest, fileBytes, "original.pdf");

        var animal = await factory.GetAsync(animalId);
        var recordId = animal.HealthRecords.First().Id;
        var originalDocFileName = animal.HealthRecords.First().Document!.FileName;

        using var multiPartContent = new MultipartFormDataContent();
        multiPartContent.Add(new StringContent(animalId.ToString()), "AnimalId");
        multiPartContent.Add(new StringContent(recordId.ToString()), "HealthRecordId");
        multiPartContent.Add(new StringContent(DateTimeOffset.UtcNow.ToString("o")), "OccurredOn");
        multiPartContent.Add(new StringContent("Updated without document changes"), "Description");

        var response = await client.PutAsync(
            UpdateAnimalHealthRequest.BuildRoute(animalId, recordId),
            multiPartContent);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var updatedAnimal = await factory.GetAsync(animalId);
        var updatedRecord = updatedAnimal.HealthRecords.First(r => r.Id == recordId);

        updatedRecord.Document.Should().NotBeNull();
        updatedRecord.Document!.FileName.Should().Be(originalDocFileName);
    }

    [Fact]
    public async Task ShouldReplaceDocument_WhenFileIsProvided()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);
        var client = Factory.CreateAuthenticatedClient(user);

        var animalId = await factory.CreateAsync(
            "2024/9110",
            "TRANS-HEALTH-10",
            "Health Replace Doc",
            AnimalSpecies.Dog,
            AnimalSex.Female);

        var originalFileBytes = Encoding.UTF8.GetBytes("Original document content");
        var addRequest = new CreateAnimalHealthRequest
        {
            AnimalId = animalId,
            OccurredOn = DateTimeOffset.UtcNow.AddDays(-1),
            Description = "Health with original document"
        };
        await AddHealthAsync(client, animalId, addRequest, originalFileBytes, "original.pdf");

        var animal = await factory.GetAsync(animalId);
        var recordId = animal.HealthRecords.First().Id;

        var newFileBytes = Encoding.UTF8.GetBytes("New document content");
        using var multiPartContent = new MultipartFormDataContent();
        multiPartContent.Add(new StringContent(animalId.ToString()), "AnimalId");
        multiPartContent.Add(new StringContent(recordId.ToString()), "HealthRecordId");
        multiPartContent.Add(new StringContent(DateTimeOffset.UtcNow.ToString("o")), "OccurredOn");
        multiPartContent.Add(new StringContent("Document replaced"), "Description");

        var fileContent = new ByteArrayContent(newFileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        multiPartContent.Add(fileContent, "DocumentFile", "new-document.pdf");

        var response = await client.PutAsync(
            UpdateAnimalHealthRequest.BuildRoute(animalId, recordId),
            multiPartContent);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var updatedAnimal = await factory.GetAsync(animalId);
        var updatedRecord = updatedAnimal.HealthRecords.First(r => r.Id == recordId);

        updatedRecord.Document.Should().NotBeNull();
        updatedRecord.Document!.FileName.Should().Be("new-document.pdf");
    }

    [Fact]
    public async Task ShouldDeleteExistingDocument_WhenDeleteDocumentIsTrue()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);
        var client = Factory.CreateAuthenticatedClient(user);

        var animalId = await factory.CreateAsync(
            "2024/9111",
            "TRANS-HEALTH-11",
            "Health Delete Existing Doc",
            AnimalSpecies.Cat,
            AnimalSex.Male);

        var fileBytes = Encoding.UTF8.GetBytes("Document to be deleted");
        var addRequest = new CreateAnimalHealthRequest
        {
            AnimalId = animalId,
            OccurredOn = DateTimeOffset.UtcNow.AddDays(-1),
            Description = "Health with document to delete"
        };
        await AddHealthAsync(client, animalId, addRequest, fileBytes, "document-to-delete.pdf");

        var animal = await factory.GetAsync(animalId);
        var recordId = animal.HealthRecords.First().Id;

        using var multiPartContent = new MultipartFormDataContent();
        multiPartContent.Add(new StringContent(animalId.ToString()), "AnimalId");
        multiPartContent.Add(new StringContent(recordId.ToString()), "HealthRecordId");
        multiPartContent.Add(new StringContent(DateTimeOffset.UtcNow.ToString("o")), "OccurredOn");
        multiPartContent.Add(new StringContent("Document deleted"), "Description");
        multiPartContent.Add(new StringContent("true"), "DeleteDocument");

        var response = await client.PutAsync(
            UpdateAnimalHealthRequest.BuildRoute(animalId, recordId),
            multiPartContent);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var updatedAnimal = await factory.GetAsync(animalId);
        var updatedRecord = updatedAnimal.HealthRecords.First(r => r.Id == recordId);

        updatedRecord.Document.Should().BeNull();
    }
}