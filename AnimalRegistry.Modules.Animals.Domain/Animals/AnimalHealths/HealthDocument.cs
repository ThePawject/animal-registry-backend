using AnimalRegistry.Shared.DDD;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimalRegistry.Modules.Animals.Domain.Animals.AnimalHealths;

public sealed class HealthDocument : Entity
{
    private HealthDocument()
    {
    }

    private HealthDocument(Guid id, string blobPath, string fileName, string contentType, DateTimeOffset uploadedOn)
    {
        Id = id;
        BlobPath = blobPath;
        FileName = fileName;
        ContentType = contentType;
        UploadedOn = uploadedOn;
    }

    public string BlobPath { get; private set; } = null!;
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public DateTimeOffset UploadedOn { get; private set; }

    [NotMapped] public string? Url { get; internal set; }

    public static HealthDocument Create(string blobPath, string fileName, string contentType)
    {
        return new HealthDocument(Guid.NewGuid(), blobPath, fileName, contentType, DateTimeOffset.UtcNow);
    }
}
