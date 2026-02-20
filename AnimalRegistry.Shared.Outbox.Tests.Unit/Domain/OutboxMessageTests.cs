using AnimalRegistry.Shared.DDD;
using AnimalRegistry.Shared.Outbox.Domain;
using FluentAssertions;

namespace AnimalRegistry.Shared.Outbox.Tests.Unit.Domain;

public class OutboxMessageTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateOutboxMessage()
    {
        var message = OutboxMessage.Create(
            "TestEvent",
            "{\"data\":\"test\"}",
            "Animals");

        message.Should().NotBeNull();
        message.MessageType.Should().Be("TestEvent");
        message.MessageData.Should().Be("{\"data\":\"test\"}");
        message.ModuleName.Should().Be("Animals");
        message.Status.Should().Be(OutboxMessageStatus.Pending);
        message.RetryCount.Should().Be(0);
        message.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithEmptyMessageType_ShouldThrowBusinessRuleValidationException()
    {
        var act = () => OutboxMessage.Create("", "{\"data\":\"test\"}", "Animals");

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Message type cannot be null or whitespace");
    }

    [Fact]
    public void Create_WithEmptyMessageData_ShouldThrowBusinessRuleValidationException()
    {
        var act = () => OutboxMessage.Create("TestEvent", "", "Animals");

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Message data cannot be null or whitespace");
    }

    [Fact]
    public void Create_WithEmptyModuleName_ShouldThrowBusinessRuleValidationException()
    {
        var act = () => OutboxMessage.Create("TestEvent", "{\"data\":\"test\"}", "");

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Module name cannot be null or whitespace");
    }

    [Fact]
    public void MarkAsProcessing_ShouldChangeStatus()
    {
        var message = OutboxMessage.Create("TestEvent", "{\"data\":\"test\"}", "Animals");

        message.MarkAsProcessing();

        message.Status.Should().Be(OutboxMessageStatus.Processing);
    }

    [Fact]
    public void MarkAsProcessed_ShouldChangeStatusAndSetProcessedAt()
    {
        var message = OutboxMessage.Create("TestEvent", "{\"data\":\"test\"}", "Animals");

        message.MarkAsProcessed();

        message.Status.Should().Be(OutboxMessageStatus.Processed);
        message.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        message.Error.Should().BeNull();
    }

    [Fact]
    public void MarkAsFailed_WithValidData_ShouldIncreaseRetryCountAndSetError()
    {
        var message = OutboxMessage.Create("TestEvent", "{\"data\":\"test\"}", "Animals");
        var initialRetryCount = message.RetryCount;
        var retryDelay = TimeSpan.FromSeconds(10);

        message.MarkAsFailed("Test error", retryDelay);

        message.Status.Should().Be(OutboxMessageStatus.Failed);
        message.RetryCount.Should().Be(initialRetryCount + 1);
        message.Error.Should().Be("Test error");
        message.NextRetryAt.Should().BeCloseTo(DateTime.UtcNow.Add(retryDelay), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsFailed_WithEmptyError_ShouldThrowBusinessRuleValidationException()
    {
        var message = OutboxMessage.Create("TestEvent", "{\"data\":\"test\"}", "Animals");

        var act = () => message.MarkAsFailed("", TimeSpan.FromSeconds(10));

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Error message cannot be null or whitespace");
    }

    [Fact]
    public void MarkAsFailed_WithNegativeRetryDelay_ShouldThrowBusinessRuleValidationException()
    {
        var message = OutboxMessage.Create("TestEvent", "{\"data\":\"test\"}", "Animals");

        var act = () => message.MarkAsFailed("Test error", TimeSpan.FromSeconds(-1));

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Retry delay cannot be negative");
    }

    [Fact]
    public void CanRetry_WhenBelowMaxRetryCount_ShouldReturnTrue()
    {
        var message = OutboxMessage.Create("TestEvent", "{\"data\":\"test\"}", "Animals");
        message.MarkAsFailed("Error", TimeSpan.Zero);

        message.CanRetry(5).Should().BeTrue();
    }

    [Fact]
    public void CanRetry_WhenReachedMaxRetryCount_ShouldReturnFalse()
    {
        var message = OutboxMessage.Create("TestEvent", "{\"data\":\"test\"}", "Animals");
        
        for (int i = 0; i < 5; i++)
        {
            message.MarkAsFailed("Error", TimeSpan.Zero);
        }

        message.CanRetry(5).Should().BeFalse();
    }

    [Fact]
    public void ResetForRetry_ShouldResetStatusAndNextRetryAt()
    {
        var message = OutboxMessage.Create("TestEvent", "{\"data\":\"test\"}", "Animals");
        message.MarkAsFailed("Error", TimeSpan.FromSeconds(10));

        message.ResetForRetry();

        message.Status.Should().Be(OutboxMessageStatus.Pending);
        message.NextRetryAt.Should().BeNull();
    }
}
