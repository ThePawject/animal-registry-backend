using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateExpiringVaccinationsReport(IMediator mediator)
    : Endpoint<GenerateExpiringVaccinationsReportRequest>
{
    public override void Configure()
    {
        Get(GenerateExpiringVaccinationsReportRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Options(b => b.RequireShelterFeature(AnimalVaccinationFeature.FeatureKey));
        Summary(s =>
        {
            s.Summary = "Generate expiring vaccinations report";
            s.Description = "PDF with vaccinations whose validity ends within the given number of days.";
        });
    }

    public override async Task HandleAsync(GenerateExpiringVaccinationsReportRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GenerateExpiringVaccinationsReportCommand(req.ExpiresInDays, req.IncludeExpired, req.InShelterOnly),
            ct);

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
