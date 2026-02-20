using AnimalRegistry.Modules.Audit.Domain.AuditEntries;
using AnimalRegistry.Shared.DDD;
using FluentAssertions;

namespace AnimalRegistry.Modules.Audit.Tests.Unit.Domain;

public class AuditMetadataTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateMetadata()
    {
        var metadata = AuditMetadata.Create(
            "user-123",
            "user@test.com",
            "shelter-1",
            "192.168.1.1",
            "Mozilla/5.0");

        metadata.Should().NotBeNull();
        metadata.UserId.Should().Be("user-123");
        metadata.Email.Should().Be("user@test.com");
        metadata.ShelterId.Should().Be("shelter-1");
        metadata.IpAddress.Should().Be("192.168.1.1");
        metadata.UserAgent.Should().Be("Mozilla/5.0");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrowBusinessRuleValidationException()
    {
        var act = () => AuditMetadata.Create("", "user@test.com", "shelter-1");

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("User ID cannot be null or whitespace");
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldThrowBusinessRuleValidationException()
    {
        var act = () => AuditMetadata.Create("user-123", "", "shelter-1");

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Email cannot be null or whitespace");
    }

    [Fact]
    public void Create_WithEmptyShelterId_ShouldThrowBusinessRuleValidationException()
    {
        var act = () => AuditMetadata.Create("user-123", "user@test.com", "");

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Shelter ID cannot be null or whitespace");
    }

    [Fact]
    public void Create_WithoutOptionalFields_ShouldCreateMetadata()
    {
        var metadata = AuditMetadata.Create("user-123", "user@test.com", "shelter-1");

        metadata.IpAddress.Should().BeNull();
        metadata.UserAgent.Should().BeNull();
    }

    [Fact]
    public void Equals_WithSameValues_ShouldBeEqual()
    {
        var metadata1 = AuditMetadata.Create("user-123", "user@test.com", "shelter-1", "192.168.1.1", "Mozilla/5.0");
        var metadata2 = AuditMetadata.Create("user-123", "user@test.com", "shelter-1", "192.168.1.1", "Mozilla/5.0");

        metadata1.Should().Be(metadata2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldNotBeEqual()
    {
        var metadata1 = AuditMetadata.Create("user-123", "user@test.com", "shelter-1");
        var metadata2 = AuditMetadata.Create("user-456", "user2@test.com", "shelter-2");

        metadata1.Should().NotBe(metadata2);
    }
}
