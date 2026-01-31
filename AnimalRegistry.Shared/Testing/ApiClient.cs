using System.Net.Http.Json;

namespace AnimalRegistry.Shared.Testing;

public sealed class ApiClient(HttpClient client)
{
    public Task<HttpResponseMessage> PostJsonAsync<T>(string path, T body)
        => client.PostAsJsonAsync(path, body);

    public Task<HttpResponseMessage> GetAsync(string path)
        => client.GetAsync(path);

    public Task<HttpResponseMessage> PutJsonAsync<T>(string path, T body)
        => client.PutAsJsonAsync(path, body);

    public Task<HttpResponseMessage> DeleteAsync(string path)
        => client.DeleteAsync(path);
}
