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
        var imageOptimizationService = Substitute.For<IImageOptimizationService>();
        _service = new BlobStorageService(options, containerClient, imageOptimizationService);
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

        var containerClient = Substitute.For<BlobContainerClient>();
        _imageOptimizationService = Substitute.For<IImageOptimizationService>();
        _service = new BlobStorageService(options, containerClient, _imageOptimizationService);
    }

    [Theory]
    [InlineData("photo.bmp")]
    [InlineData("photo.gif")]
    [InlineData("photo.pdf")]
    [InlineData("photo.txt")]
    [InlineData("photo.exe")]
    public async Task UploadImageToWebpAsync_With_Invalid_Image_File_Should_Return_ValidationError(string fileName)
    {
        var content = new MemoryStream([1, 2, 3]);

        _imageOptimizationService
            .OptimizeImageAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Result<Stream>.ValidationError(
                "Invalid image file. The file format is not supported or the file is corrupted."));

        var result = await _service.UploadImageToWebpAsync(fileName, content, "image/jpeg", "shelter-1", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid image file");
    }

    [Fact]
    public async Task UploadImageToWebpAsync_With_File_Too_Large_Should_Return_ValidationError()
    {
        const string fileName = "photo.jpg";
        var content = new MemoryStream(new byte[21 * 1024 * 1024]);

        var result = await _service.UploadImageToWebpAsync(fileName, content, "image/jpeg", "shelter-1", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("File is too large");
    }

    [Theory]
    [InlineData("document.pdf", "application/pdf")]
    [InlineData("document.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("document.doc", "application/msword")]
    [InlineData("document.txt", "text/plain")]
    [InlineData("document.jpg", "image/jpeg")]
    [InlineData("document.png", "image/png")]
    public async Task UploadDocumentAsync_With_Valid_Document_Should_Return_BlobPath(string fileName, string contentType)
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
        var blobClient = Substitute.For<BlobClient>();
        containerClient.GetBlobClient(Arg.Any<string>()).Returns(blobClient);

        var service = new BlobStorageService(options, containerClient, _imageOptimizationService);

        var content = new MemoryStream([1, 2, 3]);

        var result = await service.UploadDocumentAsync(fileName, content, contentType, "shelter-1", Guid.Parse("00000000-0000-0000-0000-000000000001"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().StartWith("shelter-1/00000000-0000-0000-0000-000000000001/");
    }

    [Fact]
    public async Task UploadDocumentAsync_With_File_Too_Large_Should_Return_ValidationError()
    {
        var content = new MemoryStream(new byte[21 * 1024 * 1024]);

        var result = await _service.UploadDocumentAsync("document.pdf", content, "application/pdf", "shelter-1", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("File is too large");
    }
}