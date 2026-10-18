using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

internal sealed class CreateAnimalVaccination(IMediator mediator)
    : Endpoint<CreateAnimalVaccinationRequest, AnimalVaccinationDto>
{
    public override void Configure()
    {
        Post(CreateAnimalVaccinationRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Options(b => b.RequireShelterFeature(AnimalVaccinationFeature.FeatureKey));
        Summary(s =>
        {
            s.Summary = "Add vaccination";
            s.Description =
                "Adds a vaccination to an animal. Only the vaccine name and date are required. " +
                "ReplacesVaccinationId marks an earlier vaccination of the same animal as replaced.";
        });
    }

    public override async Task HandleAsync(CreateAnimalVaccinationRequest req, CancellationToken ct)
    {
        var data = new AnimalVaccinationData(req.VaccineName, req.AdministeredOn!.Value, req.ValidUntil,
            req.Veterinarian, req.BatchNumber, req.Notes);
        var result = await mediator.Send(
            new CreateAnimalVaccinationCommand(req.AnimalId, data, req.ReplacesVaccinationId), ct);
        await this.SendResultAsync(result, ct);
    }
}
