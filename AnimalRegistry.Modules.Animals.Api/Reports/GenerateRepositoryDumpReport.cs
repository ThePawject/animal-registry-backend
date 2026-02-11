using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Reports;

internal sealed class GenerateRepositoryDumpReport(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/reports/animals/dump");
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Summary(s =>
        {
            s.Summary = "Generate repository dump report";
            s.Description = "Generates a complete PDF dump of all animals in the repository.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new GenerateRepositoryDumpReportCommand(), ct);

        if (await this.SendResultIfFailureAsync(result, ct))
            return;

        var response = result.Value!;
        HttpContext.Response.ContentType = response.ContentType;
        HttpContext.Response.Headers.ContentDisposition = $"attachment; filename=\"{response.FileName}\"";
        await HttpContext.Response.Body.WriteAsync(response.Data, ct);
    }
}
