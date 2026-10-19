using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

internal sealed class ListAnimalVaccinations(IMediator mediator)
    : Endpoint<ListAnimalVaccinationsRequest, IReadOnlyList<AnimalVaccinationDto>>
{
    public override void Configure()
    {
        Get(ListAnimalVaccinationsRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Options(b => b.RequireShelterFeature(AnimalVaccinationFeature.FeatureKey));
        Summary(s =>
        {
            s.Summary = "List animal vaccinations";
            s.Description = "Vaccinations of an animal, newest first, with status Active, Expired or Replaced.";
        });
    }

    public override async Task HandleAsync(ListAnimalVaccinationsRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new ListAnimalVaccinationsQuery(req.AnimalId), ct);
        await this.SendResultAsync(result, ct);
    }
}
