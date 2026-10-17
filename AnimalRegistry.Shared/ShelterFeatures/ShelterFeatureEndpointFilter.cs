using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Shared.ShelterFeatures;

public sealed class ShelterFeatureEndpointFilter(string featureKey) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var toggles = context.HttpContext.RequestServices.GetRequiredService<IShelterFeatureToggles>();

        if (!await toggles.IsEnabledAsync(featureKey, context.HttpContext.RequestAborted))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                detail: $"Module '{featureKey}' is not enabled for this shelter.");
        }

        return await next(context);
    }
}

public static class ShelterFeatureEndpointExtensions
{
    public static RouteHandlerBuilder RequireShelterFeature(this RouteHandlerBuilder builder, string featureKey)
    {
        return builder.AddEndpointFilter(new ShelterFeatureEndpointFilter(featureKey));
    }
}
