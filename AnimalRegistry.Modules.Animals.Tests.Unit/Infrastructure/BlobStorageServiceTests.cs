using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Infrastructure;
using AnimalRegistry.Modules.Animals.Infrastructure.Services;
using AnimalRegistry.Shared;
using Azure.Storage.Blobs;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Infrastructure;

public class BlobStorageServiceTests
{
    private readonly IImageOptimizationService _imageOptimizationService;
    private readonly BlobStorageService _service;

    public BlobStorageServiceTests()
    {
        var settings = new BlobStorageSettings
        {
            ConnectionString = "UseDevelopmentStorage=true",
            ContainerName = "test-container",
            AccountName = "testaccount",
        };
        var options = Substitute.For<IOptions<BlobStorageSettings>>();
        options.Value.Returns(settings);

        var containerClient = Substitute.For<BlobContainerClient>();
        _imageOptimizationService = Substitute.For<IImageOptimizationService>();
        _service = new BlobStorageService(options, containerClient, _imageOptimizationService);
    }

    [Theory]
    [InlineData("photo.jpg")]
    [InlineData("photo.jpeg")]
    [InlineData("photo.png")]
    [InlineData("photo.webp")]
    [InlineData("photo.JPG")]
    [InlineData("photo.PNG")]
    public void GetBlobUrl_Should_Return_Correct_Url(string blobPath)
    {
        var url = _service.GetBlobUrl(blobPath);

        url.Should().Be($"https://testaccount.blob.core.windows.net/test-container/{blobPath}");
    }

    [Fact]
    public void GetBlobUrl_With_Path_Containing_Folders_Should_Return_Correct_Url()
    {
        var blobPath = "shelter-id/animal-id/20240209123456789_photo.jpg";

        var url = _service.GetBlobUrl(blobPath);

        url.Should()
            .Be(
                "https://testaccount.blob.core.windows.net/test-container/shelter-id/animal-id/20240209123456789_photo.jpg");
    }
}

public class BlobStorageValidationTests
{
    private readonly BlobContainerClient _containerClient;
    private readonly IImageOptimizationService _imageOptimizationService;
    private readonly BlobStorageService _service;

    public BlobStorageValidationTests()
    {
        var settings = new BlobStorageSettings
        {
            ConnectionString = "UseDevelopmentStorage=true",
            ContainerName = "test-container",
            AccountName = "testaccount",
        };
        var options = Substitute.For<IOptions<BlobStorageSettings>>();
        options.Value.Returns(settings);

        _containerClient = Substitute.For<BlobContainerClient>();
        _imageOptimizationService = Substitute.For<IImageOptimizationService>();
        _service = new BlobStorageService(options, _containerClient, _imageOptimizationService);
    }

    [Theory]
    [InlineData("photo.bmp")]
    [InlineData("photo.gif")]
    [InlineData("photo.pdf")]
    [InlineData("photo.txt")]
    [InlineData("photo.exe")]
    public async Task UploadAsync_With_Invalid_Image_File_Should_Return_ValidationError(string fileName)
    {
        var content = new MemoryStream([1, 2, 3]);

        _imageOptimizationService
            .OptimizeImageAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Result<Stream>.ValidationError(
                "Invalid image file. The file format is not supported or the file is corrupted."));

        var result = await _service.UploadAsync(fileName, content, "image/jpeg", "shelter-1", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid image file");
    }

    [Fact]
    public async Task UploadAsync_With_File_Too_Large_Should_Return_ValidationError()
    {
        const string fileName = "photo.jpg";
        var content = new MemoryStream(new byte[21 * 1024 * 1024]);

        var result = await _service.UploadAsync(fileName, content, "image/jpeg", "shelter-1", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("File is too large");
    }
}