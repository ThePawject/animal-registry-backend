using AnimalRegistry.Modules.Animals.Application;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services;

internal sealed class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly BlobStorageSettings _settings;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private static readonly Dictionary<string, string> ExtensionToContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".webp", "image/webp" }
    };

    public BlobStorageService(IOptions<BlobStorageSettings> options)
    {
        _settings = options.Value;
        var blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadAsync(
        string fileName, 
        Stream content, 
        string contentType,
        string shelterId,
        Guid animalId,
        CancellationToken cancellationToken = default)
    {
        ValidateFile(fileName, content);

        var safeFileName = SanitizeFileName(fileName);
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssfff");
        var blobPath = $"{shelterId}/{animalId}/{timestamp}_{safeFileName}";
        
        var blobClient = _containerClient.GetBlobClient(blobPath);

        var blobHttpHeaders = new BlobHttpHeaders 
        { 
            ContentType = contentType,
            CacheControl = "public, max-age=31536000"
        };

        await blobClient.UploadAsync(content, new BlobUploadOptions 
        { 
            HttpHeaders = blobHttpHeaders,
            Metadata = new Dictionary<string, string>
            {
                { "shelterId", shelterId },
                { "animalId", animalId.ToString() },
                { "originalFileName", fileName }
            }
        }, cancellationToken);

        return blobPath;
    }

    public async Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public string GetBlobUrl(string blobPath)
    {
        return _settings.GetBlobUrl(blobPath);
    }

    private void ValidateFile(string fileName, Stream content)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                $"Invalid file extension: '{extension}'. Allowed: {string.Join(", ", AllowedExtensions)}",
                nameof(fileName));
        }

        const long maxSize = 10 * 1024 * 1024;
        if (content.Length > maxSize)
        {
            throw new ArgumentException(
                $"File is too large: {content.Length / 1024 / 1024}MB. Maximum size: 10MB",
                nameof(content));
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        name = name.Replace(" ", "_");
        name = Regex.Replace(name, "[^a-zA-Z0-9._-]", "_");
        
        if (string.IsNullOrWhiteSpace(name) || name == "_")
        {
            name = $"photo_{Guid.NewGuid():N}";
        }
        
        return name;
    }
}
