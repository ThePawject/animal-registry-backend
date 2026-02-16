using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Shared;
using Microsoft.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Services;

internal sealed class ImageOptimizationService : IImageOptimizationService
{
    private const int WebpQuality = 75;
    private const int MaxImageDimension = 2048;

    private readonly RecyclableMemoryStreamManager _memoryStreamManager = new();

    public async Task<Result<Stream>> OptimizeImageAsync(Stream sourceStream,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (sourceStream.CanSeek)
            {
                sourceStream.Position = 0;
            }

            using var image = await Image.LoadAsync(sourceStream, cancellationToken);

            image.Metadata.ExifProfile = null;
            image.Metadata.IptcProfile = null;
            image.Metadata.XmpProfile = null;

            var originalWidth = image.Width;
            var originalHeight = image.Height;

            var outputStream = _memoryStreamManager.GetStream();

            var encoder = new WebpEncoder { Quality = WebpQuality, FileFormat = WebpFileFormatType.Lossy };

            if (originalWidth > MaxImageDimension || originalHeight > MaxImageDimension)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max, Size = new Size(MaxImageDimension, MaxImageDimension),
                }));
            }

            await image.SaveAsWebpAsync(outputStream, encoder, cancellationToken);

            outputStream.Position = 0;

            return Result<Stream>.Success(outputStream);
        }
        catch (UnknownImageFormatException)
        {
            return Result<Stream>.ValidationError(
                "Invalid image file. The file format is not supported or the file is corrupted.");
        }
        catch (InvalidImageContentException)
        {
            return Result<Stream>.ValidationError("Invalid image file. The file content is corrupted or incomplete.");
        }
        catch (Exception ex)
        {
            return Result<Stream>.ValidationError($"Failed to process image: {ex.Message}");
        }
    }
}