namespace AnimalRegistry.Modules.Animals.Application;

public interface IBlobStorageService
{
    Task<string> UploadAsync(string fileName, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string blobUrl, CancellationToken cancellationToken = default);
}
