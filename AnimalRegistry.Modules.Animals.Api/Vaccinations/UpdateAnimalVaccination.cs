using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

internal sealed class UpdateAnimalVaccination(IMediator mediator)
    : Endpoint<UpdateAnimalVaccinationRequest, AnimalVaccinationDto>
{
    public override void Configure()
    {
        Put(UpdateAnimalVaccinationRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Options(b => b.RequireShelterFeature(AnimalVaccinationFeature.FeatureKey));
        Summary(s =>
        {
            s.Summary = "Update vaccination";
            s.Description =
                "Replaces vaccination data, including which earlier vaccination it replaces. " +
                "The attachment is managed by the attachment endpoints.";
        });
    }

    public override async Task HandleAsync(UpdateAnimalVaccinationRequest req, CancellationToken ct)
    {
        var data = new AnimalVaccinationData(req.VaccineName, req.AdministeredOn!.Value, req.ValidUntil,
            req.Veterinarian, req.BatchNumber, req.Notes);
        var result = await mediator.Send(
            new UpdateAnimalVaccinationCommand(req.AnimalId, req.VaccinationId, data, req.ReplacesVaccinationId), ct);
        await this.SendResultAsync(result, ct);
    }
}
