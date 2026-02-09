using AnimalRegistry.Modules.Animals.Application.AnimalHealth;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.AnimalHealth;

public sealed class UpdateAnimalHealth(IMediator mediator) : Endpoint<UpdateAnimalHealthRequest>
{
    public override void Configure()
    {
        Put(UpdateAnimalHealthRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Summary(s =>
        {
            s.Summary = "Updates an existing health record for an animal.";
            s.Description = "Updates an existing health record for an animal.";
        });
    }

    public override async Task HandleAsync(UpdateAnimalHealthRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateAnimalHealthCommand(
            req.AnimalId,
            req.HealthRecordId,
            req.OccurredOn,
            req.Description
        ), ct);

        await this.SendResultAsync(result, ct);
    }
}