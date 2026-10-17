namespace AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;

public sealed class VaccinationAttachment
{
    private VaccinationAttachment()
    {
    }

    private VaccinationAttachment(string blobPath, string fileName, string contentType, DateTimeOffset uploadedOn)
    {
        BlobPath = blobPath;
        FileName = fileName;
        ContentType = contentType;
        UploadedOn = uploadedOn;
    }

    public string BlobPath { get; private set; } = null!;
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public DateTimeOffset UploadedOn { get; private set; }

    public static VaccinationAttachment Create(string blobPath, string fileName, string contentType,
        DateTimeOffset uploadedOn)
    {
        return new VaccinationAttachment(blobPath, fileName, contentType, uploadedOn);
    }
}
