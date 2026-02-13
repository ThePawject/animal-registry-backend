using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateEventReport(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/reports/events");
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Summary(s =>
        {
            s.Summary = "Generate event report";
            s.Description = "Generates a PDF report of animal events grouped by species and time periods.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new GenerateEventReportCommand(), ct);

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