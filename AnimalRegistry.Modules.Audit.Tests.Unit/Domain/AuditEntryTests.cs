using AnimalRegistry.Modules.Audit.Domain.AuditEntries;
using AnimalRegistry.Shared.DDD;
using FluentAssertions;

namespace AnimalRegistry.Modules.Audit.Tests.Unit.Domain;

public class AuditEntryTests
{
    [Fact]
    public void CreateForDomainEvent_WithValidData_ShouldCreateAuditEntry()
    {
        var metadata = AuditMetadata.Create("user-123", "user@test.com", "shelter-1");
        
        var auditEntry = AuditEntry.CreateForDomainEvent(
            "TestEvent",
            "{\"data\":\"test\"}",
            metadata);

        auditEntry.Should().NotBeNull();
        auditEntry.Type.Should().Be(AuditEntryType.DomainEvent);
        auditEntry.EntityType.Should().Be("TestEvent");
        auditEntry.EntityData.Should().Be("{\"data\":\"test\"}");
        auditEntry.Metadata.Should().Be(metadata);
        auditEntry.IsSuccess.Should().BeTrue();
        auditEntry.ExecutionTime.Should().BeNull();
        auditEntry.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void CreateForDomainEvent_WithEmptyEntityType_ShouldThrowBusinessRuleValidationException()
    {
        var metadata = AuditMetadata.Create("user-123", "user@test.com", "shelter-1");
        
        var act = () => AuditEntry.CreateForDomainEvent("", "{\"data\":\"test\"}", metadata);

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Entity type cannot be null or whitespace");
    }

    [Fact]
    public void CreateForDomainEvent_WithEmptyEntityData_ShouldThrowBusinessRuleValidationException()
    {
        var metadata = AuditMetadata.Create("user-123", "user@test.com", "shelter-1");
        
        var act = () => AuditEntry.CreateForDomainEvent("TestEvent", "", metadata);

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Entity data cannot be null or whitespace");
    }

    [Fact]
    public void CreateForCommand_WithValidData_ShouldCreateAuditEntry()
    {
        var metadata = AuditMetadata.Create("user-123", "user@test.com", "shelter-1");
        var executionTime = TimeSpan.FromMilliseconds(100);
        
        var auditEntry = AuditEntry.CreateForCommand(
            "TestCommand",
            "{\"command\":\"test\"}",
            metadata,
            executionTime,
            true);

        auditEntry.Should().NotBeNull();
        auditEntry.Type.Should().Be(AuditEntryType.Command);
        auditEntry.EntityType.Should().Be("TestCommand");
        auditEntry.EntityData.Should().Be("{\"command\":\"test\"}");
        auditEntry.Metadata.Should().Be(metadata);
        auditEntry.IsSuccess.Should().BeTrue();
        auditEntry.ExecutionTime.Should().Be(executionTime);
        auditEntry.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void CreateForCommand_WithNegativeExecutionTime_ShouldThrowBusinessRuleValidationException()
    {
        var metadata = AuditMetadata.Create("user-123", "user@test.com", "shelter-1");
        
        var act = () => AuditEntry.CreateForCommand(
            "TestCommand",
            "{\"command\":\"test\"}",
            metadata,
            TimeSpan.FromSeconds(-1),
            true);

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Execution time cannot be negative");
    }

    [Fact]
    public void CreateForCommand_WithFailure_ShouldIncludeErrorMessage()
    {
        var metadata = AuditMetadata.Create("user-123", "user@test.com", "shelter-1");
        var executionTime = TimeSpan.FromMilliseconds(50);
        var errorMessage = "Something went wrong";
        
        var auditEntry = AuditEntry.CreateForCommand(
            "TestCommand",
            "{\"command\":\"test\"}",
            metadata,
            executionTime,
            false,
            errorMessage);

        auditEntry.IsSuccess.Should().BeFalse();
        auditEntry.ErrorMessage.Should().Be(errorMessage);
    }
}
