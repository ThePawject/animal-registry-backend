using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Animals.Domain.Animals;

public sealed class AnimalPhoto : Entity
{
    private AnimalPhoto()
    {
        //For ORM
    }

    private AnimalPhoto(Guid id, string blobUrl, string fileName, bool isMain, DateTimeOffset uploadedOn)
    {
        Id = id;
        BlobUrl = blobUrl;
        FileName = fileName;
        IsMain = isMain;
        UploadedOn = uploadedOn;
    }

    public string BlobUrl { get; private set; } = null!;
    public string FileName { get; private set; } = null!;
    public bool IsMain { get; private set; }
    public DateTimeOffset UploadedOn { get; private set; }

    public static AnimalPhoto Create(string blobUrl, string fileName, bool isMain = false)
    {
        return new AnimalPhoto(Guid.NewGuid(), blobUrl, fileName, isMain, DateTimeOffset.UtcNow);
    }

    public void SetAsMain()
    {
        IsMain = true;
    }

    public void UnsetAsMain()
    {
        IsMain = false;
    }
}