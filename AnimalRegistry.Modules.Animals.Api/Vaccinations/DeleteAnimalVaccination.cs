using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

internal sealed class DeleteAnimalVaccination(IMediator mediator) : Endpoint<DeleteAnimalVaccinationRequest>
{
    public override void Configure()
    {
        Delete(DeleteAnimalVaccinationRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Options(b => b.RequireShelterFeature(AnimalVaccinationFeature.FeatureKey));
        Summary(s =>
        {
            s.Summary = "Delete vaccination";
            s.Description = "Deletes a vaccination together with its attachment.";
        });
    }

    public override async Task HandleAsync(DeleteAnimalVaccinationRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteAnimalVaccinationCommand(req.AnimalId, req.VaccinationId), ct);
        await this.SendResultAsync(result, ct);
    }
}
