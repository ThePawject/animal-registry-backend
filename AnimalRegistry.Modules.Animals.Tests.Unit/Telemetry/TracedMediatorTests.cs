using System.Diagnostics;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.Telemetry;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Telemetry;

[Collection("ActivitySource")]
public class TracedMediatorTests
{
    [Fact]
    public async Task Send_ShouldReturnInnerResult_AndMarkSpanOk()
    {
        var expected = Result<string>.Success("value");
        var mediator = new TracedMediator(new DelegateMediator(_ => expected));
        using var capture = new ActivityCapture();

        var result = await mediator.Send(new GetThingQuery());

        result.Should().BeSameAs(expected);
        var activity = capture.Stopped.Should().ContainSingle().Subject;
        activity.DisplayName.Should().Be("Operation GetThingQuery");
        activity.GetTagItem("operation.name").Should().Be("GetThingQuery");
        activity.GetTagItem("operation.status").Should().Be("Ok");
        activity.Status.Should().Be(ActivityStatusCode.Unset);
    }

    [Fact]
    public async Task Send_ShouldMarkSpanError_WhenResultIsFailure()
    {
        var mediator = new TracedMediator(new DelegateMediator(_ => Result<string>.NotFound("animal missing")));
        using var capture = new ActivityCapture();

        var result = await mediator.Send(new GetThingQuery());

        result.IsFailure.Should().BeTrue();
        var activity = capture.Stopped.Should().ContainSingle().Subject;
        activity.GetTagItem("operation.status").Should().Be("NotFound");
        activity.Status.Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription.Should().Be("animal missing");
    }

    [Fact]
    public async Task Send_ShouldRethrowException_AndRecordItOnSpan()
    {
        var exception = new InvalidOperationException("boom");
        var mediator = new TracedMediator(new DelegateMediator(_ => throw exception));
        using var capture = new ActivityCapture();

        var act = () => mediator.Send(new GetThingQuery());

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        var activity = capture.Stopped.Should().ContainSingle().Subject;
        activity.GetTagItem("operation.status").Should().Be("Exception");
        activity.Status.Should().Be(ActivityStatusCode.Error);
        activity.Events.Should().ContainSingle(e => e.Name == "exception")
            .Which.Tags.Should().Contain(tag => tag.Key == "exception.type" && (string?)tag.Value == typeof(InvalidOperationException).FullName);
    }

    [Fact]
    public async Task Send_ShouldReturnInnerResult_WhenNoListenerIsRegistered()
    {
        var expected = Result<string>.Success("value");
        var mediator = new TracedMediator(new DelegateMediator(_ => expected));

        var result = await mediator.Send(new GetThingQuery());

        result.Should().BeSameAs(expected);
    }
}

internal sealed record GetThingQuery : IRequest<Result<string>>;

internal sealed class DelegateMediator(Func<object, object> handler) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((TResponse)handler(request));
    }
}
