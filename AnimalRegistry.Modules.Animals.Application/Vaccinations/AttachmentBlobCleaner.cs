using Microsoft.Extensions.Logging;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed class AttachmentBlobCleaner(
    IBlobStorageService blobStorageService,
    ILogger<AttachmentBlobCleaner> logger)
{
    public async Task DeleteAsync(string blobPath)
    {
        try
        {
            await blobStorageService.DeleteAsync(blobPath, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not delete vaccination attachment blob {BlobPath}.", blobPath);
        }
    }
}
