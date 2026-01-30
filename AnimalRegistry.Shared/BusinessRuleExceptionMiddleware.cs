using System.Diagnostics;
using AnimalRegistry.Shared.DDD;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AnimalRegistry.Shared;

public sealed class BusinessRuleExceptionMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (BusinessRuleValidationException ex) when (!ctx.Response.HasStarted)
        {
            var pd = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Instance = $"{ctx.Request.Method} {ctx.Request.Path}",
                TraceId = Activity.Current?.Id ?? ctx.TraceIdentifier,
                Detail = ex.Message,
                Errors = [],
            };

            ctx.Response.StatusCode = pd.Status;
            ctx.Response.ContentType = "application/problem+json";
            await ctx.Response.WriteAsJsonAsync(pd);
        }
    }
}

public static class BusinessRuleExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseBusinessRuleExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<BusinessRuleExceptionMiddleware>();
    }
}