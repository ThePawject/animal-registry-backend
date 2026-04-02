using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed record HealthDocumentDto(
    Guid Id,
    string FileName,
    string ContentType,
    DateTimeOffset UploadedOn,
    string? Url = null
)
{
    public static HealthDocumentDto FromDomain(AnimalHealthDocument document)
    {
        return new HealthDocumentDto(
            document.Id,
            document.FileName,
            document.ContentType,
            document.UploadedOn,
            document.Url
        );
    }
}
