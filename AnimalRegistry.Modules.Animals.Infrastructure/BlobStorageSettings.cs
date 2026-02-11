namespace AnimalRegistry.Modules.Animals.Infrastructure;

public class BlobStorageSettings
{
    /// <summary>
    ///     Optional connection string for Blob Storage.
    ///     If provided, will use connection string authentication.
    /// </summary>
    public string? ConnectionString { get; set; }

    public string ContainerName { get; set; } = null!;
    public string AccountName { get; set; } = null!;

    public string GetBlobUrl(string blobPath)
    {
        return $"https://{AccountName}.blob.core.windows.net/{ContainerName}/{blobPath}";
    }
}