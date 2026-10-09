using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Telemetry;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Telemetry;

[Collection("ActivitySource")]
public class TracedMediatorExtensionsTests
{
    [Fact]
    public async Task AddMediatorTracing_ShouldWrapRegisteredMediator()
    {
        var expected = Result<string>.Success("value");
        var services = new ServiceCollection();
        services.AddSingleton(expected);
        services.AddScoped<IMediator, FakeMediator>();
        services.AddMediatorTracing();

        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        mediator.Should().BeOfType<TracedMediator>();
        var result = await mediator.Send(new GetThingQuery());
        result.Should().BeSameAs(expected);
    }

    [Fact]
    public void AddMediatorTracing_ShouldKeepServiceLifetime()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Result<string>.Success("x"));
        services.AddScoped<IMediator, FakeMediator>();
        services.AddMediatorTracing();

        services.Should().Contain(d => d.ServiceType == typeof(IMediator) && d.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(d => d.ServiceType == typeof(FakeMediator) && d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMediatorTracing_ShouldThrow_WhenMediatorNotRegistered()
    {
        var services = new ServiceCollection();

        var act = () => services.AddMediatorTracing();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("IMediator must be registered before AddMediatorTracing is called.");
    }
}

internal sealed class FakeMediator(Result<string> result) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((TResponse)(object)result);
    }
}
