using System.Net.Http.Json;

namespace AnimalRegistry.Shared.Testing;

public sealed class ApiClient(HttpClient client)
{
    public Task<HttpResponseMessage> PostJsonAsync<T>(string path, T body)
    {
        return client.PostAsJsonAsync(path, body);
    }

    public Task<HttpResponseMessage> PostFormAsync(string path, MultipartFormDataContent content)
    {
        return client.PostAsync(path, content);
    }

    public Task<HttpResponseMessage> GetAsync(string path)
    {
        return client.GetAsync(path);
    }

    public Task<HttpResponseMessage> PutJsonAsync<T>(string path, T body)
    {
        return client.PutAsJsonAsync(path, body);
    }

    public Task<HttpResponseMessage> PutFormAsync(string path, MultipartFormDataContent content)
    {
        return client.PutAsync(path, content);
    }

    public Task<HttpResponseMessage> DeleteAsync(string path)
    {
        return client.DeleteAsync(path);
    }
}