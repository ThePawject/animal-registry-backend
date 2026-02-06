using AnimalRegistry.Modules.Animals.Application.AnimalEvents;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.AnimalEvents;

public sealed class DeleteAnimalEvent(IMediator mediator) : Endpoint<DeleteAnimalEventRequest>
{
    public override void Configure()
    {
        Delete(DeleteAnimalEventRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Summary(s =>
        {
            s.Summary = "Deletes an event from an animal.";
            s.Description = "Deletes an event from an animal.";
        });
    }

    public override async Task HandleAsync(DeleteAnimalEventRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteAnimalEventCommand(req.AnimalId, req.EventId), ct);
        await this.SendResultAsync(result, ct);
    }
}