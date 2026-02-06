using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed record AnimalPhotoDto(
    Guid Id,
    string BlobUrl,
    string FileName,
    bool IsMain,
    DateTimeOffset UploadedOn
)
{
    public static AnimalPhotoDto FromDomain(AnimalPhoto photo) => new(
        photo.Id,
        photo.BlobUrl,
        photo.FileName,
        photo.IsMain,
        photo.UploadedOn
    );
}
