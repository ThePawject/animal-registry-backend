using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed record AnimalPhotoDto(
    Guid Id,
    string Url,
    string FileName,
    DateTimeOffset UploadedOn
)
{
    public static AnimalPhotoDto FromDomain(AnimalPhoto photo,
        IBlobStorageService blobStorageService)
    {
        return new AnimalPhotoDto(
            photo.Id,
            blobStorageService.GetBlobUrl(photo.BlobPath),
            photo.FileName,
            photo.UploadedOn
        );
    }
}