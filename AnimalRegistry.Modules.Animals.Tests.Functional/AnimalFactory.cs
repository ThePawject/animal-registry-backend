using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Shared.Testing;
using System.Net.Http.Json;

public sealed class AnimalFactory(ApiClient api)
{
    public async Task<Guid> CreateAsync(string signature, string transponder, string name, AnimalSpecies species, AnimalSex sex)
    {
        var req = new CreateAnimalRequest
        {
            Signature = signature,
            TransponderCode = transponder,
            Name = name,
            Color = "Unknown",
            Species = species,
            Sex = sex,
            BirthDate = DateTimeOffset.UtcNow.AddYears(-1),
        };

        var resp = await api.PostJsonAsync(CreateAnimalRequest.Route, req);
        resp.EnsureSuccessStatusCode();
        var created = await resp.Content.ReadFromJsonAsync<CreateAnimalCommandResponse>()
            ?? throw new InvalidOperationException("Create response null");
        return created.AnimalId;
    }

    public async Task<AnimalDto> GetAsync(Guid id)
    {
        var resp = await api.GetAsync(GetAnimalRequest.BuildRoute(id));
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync<AnimalDto>() ?? throw new InvalidOperationException("Get response null");
        return dto;
    }

    public async Task<IEnumerable<AnimalDto>> ListAsync()
    {
        var resp = await api.GetAsync(ListAnimalsRequest.Route);
        resp.EnsureSuccessStatusCode();
        var list = await resp.Content.ReadFromJsonAsync<IEnumerable<AnimalDto>>() ?? throw new InvalidOperationException("List response null");
        return list;
    }
}
