using AnimalRegistry.Modules.Animals.Application.AnimalHealth;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.AnimalHealth;

public sealed class DeleteAnimalHealth(IMediator mediator) : Endpoint<DeleteAnimalHealthRequest>
{
    public override void Configure()
    {
        Delete(DeleteAnimalHealthRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Summary(s =>
        {
            s.Summary = "Deletes a health record from an animal.";
            s.Description = "Deletes a health record from an animal.";
        });
    }

    public override async Task HandleAsync(DeleteAnimalHealthRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteAnimalHealthCommand(req.AnimalId, req.HealthRecordId), ct);
        await this.SendResultAsync(result, ct);
    }
}