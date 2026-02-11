using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class GetNextAvailableSignature(IMediator mediator)
    : Endpoint<GetNextAvailableSignatureRequest, GetNextAvailableSignatureResponse>
{
    public override void Configure()
    {
        Get("/animals/signature");
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
    }

    public override async Task HandleAsync(GetNextAvailableSignatureRequest req, CancellationToken ct)
    {
        var year = req.Year ?? DateTimeOffset.UtcNow.Year;
        var result = await mediator.Send(new GetNextAvailableSignatureQuery(year), ct);
        await this.SendResultAsync(result, ct);
    }
}

public sealed class GetNextAvailableSignatureRequest
{
    [QueryParam]
    public int? Year { get; set; }
}
