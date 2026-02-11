using AnimalRegistry.Modules.Animals.Infrastructure;
using AnimalRegistry.Modules.Animals.Infrastructure.Services;
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
            AccountName = "testaccount"
        };
        var options = Substitute.For<IOptions<BlobStorageSettings>>();
        options.Value.Returns(settings);

        var containerClient = Substitute.For<BlobContainerClient>();
        _service = new BlobStorageService(options, containerClient);
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

        url.Should().Be("https://testaccount.blob.core.windows.net/test-container/shelter-id/animal-id/20240209123456789_photo.jpg");
    }
}

public class BlobStorageValidationTests
{
    private readonly BlobStorageService _service;
    private readonly BlobContainerClient _containerClient;

    public BlobStorageValidationTests()
    {
        var settings = new BlobStorageSettings
        {
            ConnectionString = "UseDevelopmentStorage=true",
            ContainerName = "test-container",
            AccountName = "testaccount"
        };
        var options = Substitute.For<IOptions<BlobStorageSettings>>();
        options.Value.Returns(settings);

        _containerClient = Substitute.For<BlobContainerClient>();
        _service = new BlobStorageService(options, _containerClient);
    }

    [Theory]
    [InlineData("photo.bmp")]
    [InlineData("photo.gif")]
    [InlineData("photo.pdf")]
    [InlineData("photo.txt")]
    [InlineData("photo.exe")]
    public async Task UploadAsync_With_Invalid_Extension_Should_Return_ValidationError(string fileName)
    {
        var content = new MemoryStream([1, 2, 3]);

        var result = await _service.UploadAsync(fileName, content, "image/jpeg", "shelter-1", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid file extension");
    }

    [Fact]
    public async Task UploadAsync_With_File_Too_Large_Should_Return_ValidationError()
    {
        const string fileName = "photo.jpg";
        var content = new MemoryStream(new byte[11 * 1024 * 1024]);

        var result = await _service.UploadAsync(fileName, content, "image/jpeg", "shelter-1", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("File is too large");
    }

    [Theory]
    [InlineData("photo.jpg")]
    [InlineData("photo.jpeg")]
    [InlineData("photo.png")]
    [InlineData("photo.webp")]
    public async Task ValidateFile_With_Valid_Extension_Should_Not_Return_ValidationError(string fileName)
    {
        var content = new MemoryStream(new byte[1024 * 1024]);
        var blobClient = Substitute.For<BlobClient>();
        _containerClient.GetBlobClient(Arg.Any<string>()).Returns(blobClient);

        var result = await _service.UploadAsync(fileName, content, "image/jpeg", "shelter-1", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
    }
}
