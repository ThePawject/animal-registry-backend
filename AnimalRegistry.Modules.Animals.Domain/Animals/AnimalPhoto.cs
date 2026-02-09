using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals;

public sealed class AnimalPhoto : Entity
{
    private AnimalPhoto()
    {
    }

    private AnimalPhoto(Guid id, string blobPath, string fileName, DateTimeOffset uploadedOn)
    {
        Id = id;
        BlobPath = blobPath;
        FileName = fileName;
        UploadedOn = uploadedOn;
    }

    public string BlobPath { get; private set; } = null!;
    public string FileName { get; private set; } = null!;
    public DateTimeOffset UploadedOn { get; private set; }

    public static AnimalPhoto Create(string blobPath, string fileName)
    {
        return new AnimalPhoto(Guid.NewGuid(), blobPath, fileName, DateTimeOffset.UtcNow);
    }
}