using System.Diagnostics;
using AnimalRegistry.Shared.Access;
using Microsoft.AspNetCore.Builder;

namespace AnimalRegistry.Shared.Telemetry;

public static class TelemetryEnrichmentExtensions
{
    private const string UserIdClaimType = "https://ThePawject/user_id";

    public static IApplicationBuilder UseRequestTelemetryEnrichment(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var activity = Activity.Current;
            if (activity is not null)
            {
                var userId = context.User.FindFirst(UserIdClaimType)?.Value;
                var shelterId = context.User.FindFirst(ShelterAccessHandler.ShelterIdClaimType)?.Value;

                if (!string.IsNullOrEmpty(userId))
                    activity.SetTag("enduser.id", userId);

                if (!string.IsNullOrEmpty(shelterId))
                    activity.SetTag("shelter.id", shelterId);
            }

            await next(context);
        });
    }
}
