using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateVaccinationsReport(IMediator mediator) : Endpoint<GenerateVaccinationsReportRequest>
{
    public override void Configure()
    {
        Get(GenerateVaccinationsReportRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Options(b => b.RequireShelterFeature(AnimalVaccinationFeature.FeatureKey));
        Summary(s =>
        {
            s.Summary = "Generate vaccinations report";
            s.Description = "PDF with vaccinations administered between the given days, both inclusive.";
        });
    }

    public override async Task HandleAsync(GenerateVaccinationsReportRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new GenerateVaccinationsReportCommand(req.From!.Value, req.To!.Value), ct);

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
