namespace AnimalRegistry.Modules.Animals.Infrastructure;

public class BlobStorageSettings
{
    public string ConnectionString { get; set; } = null!;
    public string ContainerName { get; set; } = null!;
    public string AccountName { get; set; } = null!;

    public string GetBlobUrl(string blobPath)
    {
        return $"https://{AccountName}.blob.core.windows.net/{ContainerName}/{blobPath}";
    }
}