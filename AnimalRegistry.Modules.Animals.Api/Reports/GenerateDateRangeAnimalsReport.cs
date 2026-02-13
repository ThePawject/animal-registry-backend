using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateDateRangeAnimalsReport(IMediator mediator)
    : Endpoint<GenerateDateRangeAnimalsReportRequest>
{
    public override void Configure()
    {
        Get(GenerateDateRangeAnimalsReportRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Summary(s =>
        {
            s.Summary = "Generate date range animals report";
            s.Description = "Generates a PDF report of animals and their events within a specified date range.";
        });
    }

    public override async Task HandleAsync(GenerateDateRangeAnimalsReportRequest req, CancellationToken ct)
    {
        var command = new GenerateDateRangeAnimalsReportCommand
        {
            StartDate = req.StartDate, EndDate = req.EndDate, Species = req.Species,
        };

        var result = await mediator.Send(command, ct);

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