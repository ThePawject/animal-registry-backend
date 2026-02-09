using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Shared.Pagination;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Api;

public sealed class ListAnimalsValidatorTests
{
    private readonly PaginationSettings _defaultSettings = new()
    {
        DefaultPageSize = 20,
        MaxPageSize = 100,
        MinPageSize = 1,
        MinPage = 1
    };

    private ListAnimalsValidator CreateValidator(PaginationSettings? settings = null)
    {
        var options = Options.Create(settings ?? _defaultSettings);
        return new ListAnimalsValidator(options);
    }

    [Theory]
    [InlineData(1, 20)]
    [InlineData(1, 1)]
    [InlineData(1, 100)]
    [InlineData(10, 50)]
    public void Validate_WithValidPagination_ReturnsValid(int page, int pageSize)
    {
        var validator = CreateValidator();
        var request = new ListAnimalsRequest { Page = page, PageSize = pageSize, KeyWordSearch = "valid" };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    public void Validate_WithInvalidPage_ReturnsInvalid(int page, int pageSize)
    {
        var validator = CreateValidator();
        var request = new ListAnimalsRequest { Page = page, PageSize = pageSize, KeyWordSearch = "valid" };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Page");
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 101)]
    [InlineData(1, 1000)]
    public void Validate_WithInvalidPageSize_ReturnsInvalid(int page, int pageSize)
    {
        var validator = CreateValidator();
        var request = new ListAnimalsRequest { Page = page, PageSize = pageSize, KeyWordSearch = "valid" };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void Validate_WithCustomMaxPageSize_RespectsConfiguration()
    {
        var customSettings = new PaginationSettings
        {
            MaxPageSize = 50,
            MinPageSize = 1,
            MinPage = 1
        };
        var validator = CreateValidator(customSettings);

        var request = new ListAnimalsRequest { Page = 1, PageSize = 50, KeyWordSearch = "valid" };
        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();

        request = request with { PageSize = 51 };
        result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithKeyWordSearchTooLong_ReturnsInvalid()
    {
        var validator = CreateValidator();
        var request = new ListAnimalsRequest
        {
            Page = 1,
            PageSize = 20,
            KeyWordSearch = new string('a', 101)
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "KeyWordSearch");
    }
}
