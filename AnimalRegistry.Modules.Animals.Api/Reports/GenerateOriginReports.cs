using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateOriginMonthlyReport(IMediator mediator)
    : Endpoint<GenerateOriginMonthlyReportRequest>
{
    public override void Configure()
    {
        Get(GenerateOriginMonthlyReportRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Options(b => b.RequireShelterFeature(AnimalOriginFeature.FeatureKey));
        Summary(s =>
        {
            s.Summary = "Generate monthly origin report";
            s.Description = "PDF with animals admitted in the month, grouped by contractor municipality.";
        });
    }

    public override async Task HandleAsync(GenerateOriginMonthlyReportRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new GenerateOriginMonthlyReportCommand(req.Year, req.Month), ct);
        await OriginReportResponse.SendAsync(this, result, ct);
    }
}

internal sealed class GenerateOriginYearlyReport(IMediator mediator)
    : Endpoint<GenerateOriginYearlyReportRequest>
{
    public override void Configure()
    {
        Get(GenerateOriginYearlyReportRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Options(b => b.RequireShelterFeature(AnimalOriginFeature.FeatureKey));
        Summary(s =>
        {
            s.Summary = "Generate yearly origin summary";
            s.Description = "PDF with the number of admitted animals per contractor municipality and month.";
        });
    }

    public override async Task HandleAsync(GenerateOriginYearlyReportRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new GenerateOriginYearlyReportCommand(req.Year), ct);
        await OriginReportResponse.SendAsync(this, result, ct);
    }
}

file static class OriginReportResponse
{
    public static async Task SendAsync<TRequest>(Endpoint<TRequest> ep, Result<GenerateOriginReportResponse> result,
        CancellationToken ct)
        where TRequest : notnull
    {
        if (await ep.SendResultIfFailureAsync(result, ct))
        {
            return;
        }

        var response = result.Value!;
        ep.HttpContext.Response.ContentType = response.ContentType;
        ep.HttpContext.Response.Headers.ContentDisposition = $"attachment; filename=\"{response.FileName}\"";
        await ep.HttpContext.Response.Body.WriteAsync(response.Data, ct);
    }
}
