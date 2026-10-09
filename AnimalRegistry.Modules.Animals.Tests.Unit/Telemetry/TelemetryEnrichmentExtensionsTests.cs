using System.Diagnostics;
using System.Security.Claims;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.Telemetry;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Telemetry;

[Collection("ActivitySource")]
public class TelemetryEnrichmentExtensionsTests
{
    private const string UserIdClaimType = "https://ThePawject/user_id";

    [Fact]
    public async Task Invoke_ShouldAddUserAndShelterTags_ToCurrentSpan()
    {
        var httpContext = CreateHttpContext(
            new Claim(UserIdClaimType, "auth0|user-1"),
            new Claim(ShelterAccessHandler.ShelterIdClaimType, "shelter-42"));
        using var capture = new ActivityCapture();

        using var activity = AnimalRegistryTelemetry.Source.StartActivity("request");
        await RunPipeline(httpContext);

        activity!.GetTagItem("enduser.id").Should().Be("auth0|user-1");
        activity.GetTagItem("shelter.id").Should().Be("shelter-42");
    }

    [Fact]
    public async Task Invoke_ShouldNotAddTags_WhenClaimsAreMissing()
    {
        var httpContext = CreateHttpContext();
        using var capture = new ActivityCapture();

        using var activity = AnimalRegistryTelemetry.Source.StartActivity("request");
        await RunPipeline(httpContext);

        activity!.GetTagItem("enduser.id").Should().BeNull();
        activity.GetTagItem("shelter.id").Should().BeNull();
    }

    [Fact]
    public async Task Invoke_ShouldCallNextMiddleware_WhenNoActivityIsActive()
    {
        var httpContext = CreateHttpContext(new Claim(UserIdClaimType, "auth0|user-1"));
        var nextCalled = false;

        await RunPipeline(httpContext, () => nextCalled = true);

        nextCalled.Should().BeTrue();
    }

    private static DefaultHttpContext CreateHttpContext(params Claim[] claims)
    {
        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")),
        };
    }

    private static async Task RunPipeline(HttpContext httpContext, Action? onNext = null)
    {
        var app = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());
        app.UseRequestTelemetryEnrichment();
        app.Run(_ =>
        {
            onNext?.Invoke();
            return Task.CompletedTask;
        });

        await app.Build()(httpContext);
    }
}
