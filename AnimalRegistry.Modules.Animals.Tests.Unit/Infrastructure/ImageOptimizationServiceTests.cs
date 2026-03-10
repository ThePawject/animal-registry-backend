using AnimalRegistry.Modules.Animals.Infrastructure.Services;
using FluentAssertions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure;

public class ImageOptimizationServiceTests
{
    private readonly ImageOptimizationService _service = new();

    [Fact]
    public async Task OptimizeImageAsync_With_Valid_Jpeg_Should_Convert_To_Webp()
    {
        var inputStream = CreateTestImage(100, 100, new JpegEncoder());

        var result = await _service.OptimizeImageAsync(inputStream);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.Position = 0;
        var format = await Image.DetectFormatAsync(result.Value);
        format.Should().NotBeNull();
        format!.Name.Should().BeOneOf("WEBP", "Webp");

        await result.Value.DisposeAsync();
    }

    [Fact]
    public async Task OptimizeImageAsync_With_Valid_Png_Should_Convert_To_Webp()
    {
        var inputStream = CreateTestImage(100, 100, new PngEncoder());

        var result = await _service.OptimizeImageAsync(inputStream);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.Position = 0;
        var format = await Image.DetectFormatAsync(result.Value);
        format.Should().NotBeNull();
        format!.Name.Should().BeOneOf("WEBP", "Webp");

        await result.Value.DisposeAsync();
    }

    [Fact]
    public async Task OptimizeImageAsync_With_Webp_Should_Reoptimize()
    {
        var inputStream = CreateTestImage(100, 100, new WebpEncoder());
        var originalSize = inputStream.Length;

        var result = await _service.OptimizeImageAsync(inputStream);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.Position = 0;
        var format = await Image.DetectFormatAsync(result.Value);
        format.Should().NotBeNull();
        format!.Name.Should().BeOneOf("WEBP", "Webp");

        await result.Value.DisposeAsync();
    }

    [Fact]
    public async Task OptimizeImageAsync_Should_Remove_Exif_Metadata()
    {
        var inputStream = CreateTestImageWithMetadata(100, 100);

        var result = await _service.OptimizeImageAsync(inputStream);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.Position = 0;
        using var image = await Image.LoadAsync(result.Value);
        image.Metadata.ExifProfile.Should().BeNull();
        image.Metadata.IptcProfile.Should().BeNull();
        image.Metadata.XmpProfile.Should().BeNull();

        await result.Value.DisposeAsync();
    }

    [Fact]
    public async Task OptimizeImageAsync_Should_Reduce_File_Size()
    {
        var inputStream = CreateTestImage(1000, 1000,
            new PngEncoder { CompressionLevel = PngCompressionLevel.NoCompression });
        var originalSize = inputStream.Length;

        var result = await _service.OptimizeImageAsync(inputStream);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var optimizedSize = result.Value!.Length;
        optimizedSize.Should().BeLessThan(originalSize);

        await result.Value.DisposeAsync();
    }

    [Fact]
    public async Task OptimizeImageAsync_With_Invalid_File_Should_Return_ValidationError()
    {
        var inputStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

        var result = await _service.OptimizeImageAsync(inputStream);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid image file");
    }

    [Fact]
    public async Task OptimizeImageAsync_With_Corrupted_Image_Should_Return_ValidationError()
    {
        var inputStream = new MemoryStream(new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x02, 0x03,
        });

        var result = await _service.OptimizeImageAsync(inputStream);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid image file");
    }

    [Fact]
    public async Task OptimizeImageAsync_With_Empty_Stream_Should_Return_ValidationError()
    {
        var inputStream = new MemoryStream();

        var result = await _service.OptimizeImageAsync(inputStream);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid image file");
    }

    [Fact]
    public async Task OptimizeImageAsync_Should_Reset_Stream_Position()
    {
        var inputStream = CreateTestImage(100, 100, new JpegEncoder());
        inputStream.Position = 50;

        var result = await _service.OptimizeImageAsync(inputStream);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Position.Should().Be(0);

        await result.Value.DisposeAsync();
    }

    [Theory]
    [InlineData(100, 100)]
    [InlineData(500, 300)]
    [InlineData(1920, 1080)]
    public async Task OptimizeImageAsync_Should_Preserve_Image_Dimensions(int width, int height)
    {
        var inputStream = CreateTestImage(width, height, new JpegEncoder());

        var result = await _service.OptimizeImageAsync(inputStream);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.Position = 0;
        using var image = await Image.LoadAsync(result.Value);
        image.Width.Should().Be(width);
        image.Height.Should().Be(height);

        await result.Value.DisposeAsync();
    }

    private static MemoryStream CreateTestImage(int width, int height, IImageEncoder encoder)
    {
        var stream = new MemoryStream();
        using var image = new Image<Rgba32>(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = new Rgba32((byte)(x % 256), (byte)(y % 256), 128);
                image[x, y] = color;
            }
        }

        image.Save(stream, encoder);
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateTestImageWithMetadata(int width, int height)
    {
        var stream = new MemoryStream();
        using var image = new Image<Rgba32>(width, height);

        image.Metadata.ExifProfile = new ExifProfile();
        image.Metadata.ExifProfile.SetValue(
            ExifTag.Make,
            "Test Camera"
        );

        image.SaveAsJpeg(stream);
        stream.Position = 0;
        return stream;
    }
}