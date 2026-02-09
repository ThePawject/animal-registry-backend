namespace AnimalRegistry.Modules.Animals.Application;

public interface IBlobStorageService
{
    Task<string> UploadAsync(
        string fileName, 
        Stream content, 
        string contentType,
        string shelterId,
        Guid animalId,
        CancellationToken cancellationToken = default);
    Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default);
    string GetBlobUrl(string blobPath);
}
