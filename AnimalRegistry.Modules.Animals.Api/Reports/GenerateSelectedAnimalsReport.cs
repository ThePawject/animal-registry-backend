using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateSelectedAnimalsReport(IMediator mediator) : Endpoint<GenerateSelectedAnimalsReportRequest>
{
    public override void Configure()
    {
        Get("/reports/animals/selected");
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Summary(s =>
        {
            s.Summary = "Generate selected animals report";
            s.Description = "Generates a PDF report for a specific list of animal IDs.";
        });
    }

    public override async Task HandleAsync(GenerateSelectedAnimalsReportRequest req, CancellationToken ct)
    {
        var command = new GenerateSelectedAnimalsReportCommand
        {
            Ids = req.Ids,
        };

        var result = await mediator.Send(command, ct);

        if (await this.SendResultIfFailureAsync(result, ct))
            return;

        var response = result.Value!;
        HttpContext.Response.ContentType = response.ContentType;
        HttpContext.Response.Headers.ContentDisposition = $"attachment; filename=\"{response.FileName}\"";
        await HttpContext.Response.Body.WriteAsync(response.Data, ct);
    }
}

public sealed class GenerateSelectedAnimalsReportRequest
{
    public List<Guid> Ids { get; init; } = [];
}
