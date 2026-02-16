using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Shared;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services;

internal sealed class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly IImageOptimizationService _imageOptimizationService;
    private readonly BlobStorageSettings _settings;

    public BlobStorageService(IOptions<BlobStorageSettings> options, IImageOptimizationService imageOptimizationService)
    {
        _settings = options.Value;
        _imageOptimizationService = imageOptimizationService;

        BlobServiceClient blobServiceClient;

        if (!string.IsNullOrWhiteSpace(_settings.ConnectionString))
        {
            blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
        }
        else
        {
            var credential = new DefaultAzureCredential();
            var blobServiceUri = new Uri($"https://{_settings.AccountName}.blob.core.windows.net");
            blobServiceClient = new BlobServiceClient(blobServiceUri, credential);
        }

        _containerClient = blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }

    internal BlobStorageService(IOptions<BlobStorageSettings> options, BlobContainerClient containerClient,
        IImageOptimizationService imageOptimizationService)
    {
        _settings = options.Value;
        _containerClient = containerClient;
        _imageOptimizationService = imageOptimizationService;
    }

    public async Task<Result<string>> UploadAsync(
        string fileName,
        Stream content,
        string contentType,
        string shelterId,
        Guid animalId,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateFile(fileName, content);
        if (validationResult.IsFailure)
        {
            return Result<string>.ValidationError(validationResult.Error!);
        }

        var optimizationResult = await _imageOptimizationService.OptimizeImageAsync(content, cancellationToken);
        if (optimizationResult.IsFailure)
        {
            return Result<string>.ValidationError(optimizationResult.Error!);
        }

        var optimizedStream = optimizationResult.Value!;

        var originalName = Path.GetFileNameWithoutExtension(fileName);
        var safeFileName = SanitizeFileName(originalName) + ".webp";
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssfff");
        var blobPath = $"{shelterId}/{animalId}/{timestamp}_{safeFileName}";

        var blobClient = _containerClient.GetBlobClient(blobPath);

        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = "image/webp", CacheControl = "public, max-age=31536000",
        };

        try
        {
            await blobClient.UploadAsync(optimizedStream,
                new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders,
                    Metadata = new Dictionary<string, string>
                    {
                        { "shelterId", shelterId },
                        { "animalId", animalId.ToString() },
                        { "originalFileName", fileName },
                    },
                }, cancellationToken);

            return Result<string>.Success(blobPath);
        }
        finally
        {
            await optimizedStream.DisposeAsync();
        }
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

    private static Result ValidateFile(string fileName, Stream content)
    {
        const long maxSize = 20 * 1024 * 1024;
        if (content.Length > maxSize)
        {
            return Result.ValidationError(
                $"File is too large: {content.Length / 1024 / 1024}MB. Maximum size: 20MB");
        }

        return Result.Success();
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