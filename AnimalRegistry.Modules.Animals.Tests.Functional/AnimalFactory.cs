using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared.Pagination;
using AnimalRegistry.Shared.Testing;
using System.Net.Http.Json;

public sealed class AnimalFactory(ApiClient api)
{
    public async Task<Guid> CreateAsync(string signature, string transponder, string name, AnimalSpecies species,
        AnimalSex sex)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(signature), "Signature");
        content.Add(new StringContent(transponder), "TransponderCode");
        content.Add(new StringContent(name), "Name");
        content.Add(new StringContent("Unknown"), "Color");
        content.Add(new StringContent(((int)species).ToString()), "Species");
        content.Add(new StringContent(((int)sex).ToString()), "Sex");
        content.Add(new StringContent(DateTimeOffset.UtcNow.AddYears(-1).ToString("o")), "BirthDate");

        var resp = await api.PostFormAsync(CreateAnimalRequest.Route, content);
        if (!resp.IsSuccessStatusCode)
        {
            var errorContent = await resp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Request failed with status {resp.StatusCode}: {errorContent}");
        }

        var created = await resp.Content.ReadFromJsonAsync<CreateAnimalCommandResponse>()
                      ?? throw new InvalidOperationException("Create response null");
        return created.AnimalId;
    }

    public async Task<AnimalDto> GetAsync(Guid id)
    {
        var resp = await api.GetAsync(GetAnimalRequest.BuildRoute(id));
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync<AnimalDto>() ??
                  throw new InvalidOperationException("Get response null");
        return dto;
    }

    public async Task<PagedResult<AnimalListItemDto>> ListAsync()
    {
        var resp = await api.GetAsync(ListAnimalsRequest.Route + "?page=1&pageSize=10");
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<PagedResult<AnimalListItemDto>>() ??
                     throw new InvalidOperationException("List response null");
        return result;
    }
}