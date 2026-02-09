using AnimalRegistry.Modules.Animals.Application.AnimalEvents;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.AnimalEvents;

public sealed class UpdateAnimalEvent(IMediator mediator) : Endpoint<UpdateAnimalEventRequest>
{
    public override void Configure()
    {
        Put(UpdateAnimalEventRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Summary(s =>
        {
            s.Summary = "Updates an existing event for an animal.";
            s.Description = "Updates an existing event for an animal.";
        });
    }

    public override async Task HandleAsync(UpdateAnimalEventRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateAnimalEventCommand(
            req.AnimalId,
            req.EventId,
            req.Type,
            req.OccurredOn,
            req.Description
        ), ct);

        await this.SendResultAsync(result, ct);
    }
}