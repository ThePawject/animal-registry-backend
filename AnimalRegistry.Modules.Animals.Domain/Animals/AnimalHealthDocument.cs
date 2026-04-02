using AnimalRegistry.Shared.DDD;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimalRegistry.Modules.Animals.Domain.Animals;

public sealed class AnimalHealthDocument : Entity
{
    private AnimalHealthDocument()
    {
    }

    private AnimalHealthDocument(Guid id, Guid healthRecordId, string blobPath, string fileName, string contentType, DateTimeOffset uploadedOn)
    {
        Id = id;
        HealthRecordId = healthRecordId;
        BlobPath = blobPath;
        FileName = fileName;
        ContentType = contentType;
        UploadedOn = uploadedOn;
    }

    public Guid HealthRecordId { get; private set; }
    public string BlobPath { get; private set; } = null!;
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public DateTimeOffset UploadedOn { get; private set; }

    [NotMapped] public string? Url { get; internal set; }

    public static AnimalHealthDocument Create(Guid healthRecordId, string blobPath, string fileName, string contentType)
    {
        return new AnimalHealthDocument(Guid.NewGuid(), healthRecordId, blobPath, fileName, contentType, DateTimeOffset.UtcNow);
    }
}
