using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Void = FastEndpoints.Void;

namespace AnimalRegistry.Shared;

public static class ResultEndpointExtensions
{
    public static Task SendResultAsync<TRequest, TResponse>(
        this Endpoint<TRequest, TResponse> ep,
        Result<TResponse> result,
        CancellationToken ct = default)
        where TRequest : notnull
    {
        if (result is { IsSuccess: true, Value: not null })
        {
            return ep.HttpContext.Response.SendOkAsync(result.Value, cancellation: ct);
        }

        return result.Status switch
        {
            ResultStatus.ValidationError => SendValidationError(ep, result.Error, ct),
            ResultStatus.NotFound => ep.HttpContext.Response.SendNotFoundAsync(ct),
            _ => SendProblem(ep, StatusCodes.Status500InternalServerError, result.Error, ct),
        };
    }

    public static Task SendResultAsync<TRequest>(
        this Endpoint<TRequest> ep,
        Result result,
        CancellationToken ct = default)
        where TRequest : notnull
    {
        if (result.IsSuccess)
        {
            return ep.HttpContext.Response.SendNoContentAsync(ct);
        }

        return result.Status switch
        {
            ResultStatus.ValidationError => SendValidationError(ep, result.Error, ct),
            ResultStatus.NotFound => ep.HttpContext.Response.SendNotFoundAsync(ct),
            _ => SendProblem(ep, StatusCodes.Status500InternalServerError, result.Error, ct),
        };
    }

    private static Task<Void> SendValidationError<TRequest, TResponse>(
        Endpoint<TRequest, TResponse> ep,
        string? error,
        CancellationToken ct)
        where TRequest : notnull
    {
        ep.AddError(error ?? "Validation error");
        return ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, cancellation: ct);
    }

    private static Task<Void> SendValidationError<TRequest>(
        Endpoint<TRequest> ep,
        string? error,
        CancellationToken ct)
        where TRequest : notnull
    {
        ep.AddError(error ?? "Validation error");
        return ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, cancellation: ct);
    }

    private static Task SendProblem<TRequest, TResponse>(
        Endpoint<TRequest, TResponse> ep,
        int status,
        string? detail,
        CancellationToken ct)
        where TRequest : notnull
    {
        var ctx = ep.HttpContext;

        var pd = new ProblemDetails
        {
            Status = status,
            Instance = $"{ctx.Request.Method} {ctx.Request.Path}",
            TraceId = Activity.Current?.Id ?? ctx.TraceIdentifier,
            Detail = detail ?? "An unexpected error occurred.",
            Errors = [],
        };

        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";
        return ctx.Response.WriteAsJsonAsync(pd, ct);
    }
}
