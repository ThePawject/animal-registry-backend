using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed record AnimalPhotoDto(
    Guid Id,
    string BlobPath,
    string Url,
    string FileName,
    bool IsMain,
    DateTimeOffset UploadedOn
)
{
    public static AnimalPhotoDto FromDomain(AnimalPhoto photo, Guid? mainPhotoId,
        IBlobStorageService blobStorageService)
    {
        return new AnimalPhotoDto(
            photo.Id,
            photo.BlobPath,
            blobStorageService.GetBlobUrl(photo.BlobPath),
            photo.FileName,
            photo.Id == mainPhotoId,
            photo.UploadedOn
        );
    }
}