using AnimalRegistry.Shared;

namespace AnimalRegistry.Modules.Animals.Application;

public interface IBlobStorageService
{
    Task<Result<string>> UploadImageToWebpAsync(
        string fileName,
        Stream content,
        string contentType,
        string shelterId,
        Guid animalId,
        CancellationToken cancellationToken = default);

    Task<Result<string>> UploadDocumentAsync(
        string fileName,
        Stream content,
        string contentType,
        string shelterId,
        Guid animalId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default);
    string GetBlobUrl(string blobPath);
}