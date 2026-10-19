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

        if (await this.SendResultIfFailureAsync(result, ct))
        {
            return;
        }

        var response = result.Value!;
        HttpContext.Response.ContentType = response.ContentType;
        HttpContext.Response.Headers.ContentDisposition = $"attachment; filename=\"{response.FileName}\"";
        await HttpContext.Response.Body.WriteAsync(response.Data, ct);
    }
}
