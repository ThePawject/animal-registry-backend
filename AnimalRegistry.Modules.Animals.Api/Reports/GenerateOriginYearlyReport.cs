using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

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
