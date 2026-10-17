using AnimalRegistry.Modules.Animals.Api.Municipalities;
using AnimalRegistry.Modules.Animals.Application.Municipalities;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Infrastructure;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Modules.Shelters.Api;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Origin;

public abstract class OriginModuleTestBase : IntegrationTestBase
{
    protected const string FeatureKey = "animal-origin";
    protected static readonly DateTimeOffset AdmissionDate = new(2026, 3, 15, 10, 0, 0, TimeSpan.FromHours(1));

    protected readonly string ShelterId = $"test-shelter-origin-{Guid.NewGuid():N}";
    protected readonly string OtherShelterId = $"test-shelter-other-{Guid.NewGuid():N}";
    protected readonly HttpClient Admin;
    protected readonly HttpClient Worker;
    protected readonly AnimalFactory Animals;

    protected OriginModuleTestBase(ApiTestFixture fixture) : base(fixture)
    {
        Admin = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(ShelterId));
        Worker = Factory.CreateAuthenticatedClient(TestUser.WithShelterAccess(ShelterId));
        Animals = new AnimalFactory(new ApiClient(Admin));
    }

    protected static Task<HttpResponseMessage> PutModuleAsync(HttpClient client, bool isEnabled,
        string key = FeatureKey)
    {
        return client.PutAsJsonAsync(SetShelterFeatureStateRequest.BuildRoute(key), new { IsEnabled = isEnabled });
    }

    protected async Task<HttpClient> CreateOtherShelterAdminWithModuleAsync()
    {
        var client = Factory.CreateAuthenticatedClient(TestUser.WithShelterAdmin(OtherShelterId));
        (await PutModuleAsync(client, true)).EnsureSuccessStatusCode();
        return client;
    }

    protected async Task SetModuleAsync(bool isEnabled)
    {
        (await PutModuleAsync(Admin, isEnabled)).EnsureSuccessStatusCode();
    }

    protected async Task<MunicipalityDto> CreateMunicipalityAsync(string name, HttpClient? client = null)
    {
        var response = await (client ?? Admin).PostAsJsonAsync(CreateMunicipalityRequest.Route, new { Name = name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MunicipalityDto>())!;
    }

    protected async Task<MunicipalityDto> CreateArchivedMunicipalityAsync(string name)
    {
        var municipality = await CreateMunicipalityAsync(name);
        await ArchiveMunicipalityAsync(municipality.Id);
        return municipality with { IsArchived = true };
    }

    protected async Task ArchiveMunicipalityAsync(Guid id)
    {
        (await Admin.DeleteAsync(ArchiveMunicipalityRequest.BuildRoute(id))).EnsureSuccessStatusCode();
    }

    protected static Dictionary<string, string> OriginFields(
        Guid foundId,
        Guid contractorId,
        AdmissionMethod method = AdmissionMethod.Abandoned,
        string? street = "Leśna 1",
        DateTimeOffset? admissionDate = null)
    {
        var origin = new Dictionary<string, object?>
        {
            ["admissionDate"] = admissionDate ?? AdmissionDate,
            ["admissionMethod"] = (int)method,
            ["foundStreet"] = street,
            ["foundMunicipalityId"] = foundId,
            ["contractorMunicipalityId"] = contractorId,
        };

        return new Dictionary<string, string> { ["Origin"] = JsonSerializer.Serialize(origin) };
    }

    protected Task<Guid> CreateAnimalAsync(string signature, IReadOnlyDictionary<string, string>? origin = null)
    {
        return Animals.CreateAsync(signature, $"t-{signature}", "Burek", "Mix", "none", AnimalSpecies.Dog,
            AnimalSex.Male, extraFields: origin);
    }

    protected Task<HttpResponseMessage> PostAnimalAsync(string signature,
        IReadOnlyDictionary<string, string>? origin = null)
    {
        return Animals.PostCreateAsync(signature, $"t-{signature}", "Burek", "Mix", "none", AnimalSpecies.Dog,
            AnimalSex.Male, extraFields: origin);
    }

    protected Task<HttpResponseMessage> PutAnimalAsync(Guid id, string signature,
        IReadOnlyDictionary<string, string>? origin = null, string name = "Burek")
    {
        return Animals.PutUpdateAsync(id, signature, $"t-{signature}", name, "Mix", "none", AnimalSpecies.Dog,
            AnimalSex.Male, [], extraFields: origin);
    }

    private protected async Task<T> WithDbAsync<T>(Func<AnimalsDbContext, Task<T>> query)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnimalsDbContext>();
        return await query(db);
    }

    protected async Task<T> AsShelterUserAsync<TService, T>(Func<TService, Task<T>> action)
        where TService : notnull
    {
        using var scope = Factory.Services.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var previous = accessor.HttpContext;
        accessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ShelterAccessHandler.ShelterIdClaimType, ShelterId)], "Test")),
        };

        try
        {
            return await action(scope.ServiceProvider.GetRequiredService<TService>());
        }
        finally
        {
            accessor.HttpContext = previous;
        }
    }
}
