using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

internal sealed class ListExpiringVaccinations(IMediator mediator)
    : Endpoint<ListExpiringVaccinationsRequest, IReadOnlyList<VaccinationWithAnimalDto>>
{
    public override void Configure()
    {
        Get(ListExpiringVaccinationsRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Options(b => b.RequireShelterFeature(AnimalVaccinationFeature.FeatureKey));
        Summary(s =>
        {
            s.Summary = "List expiring vaccinations";
            s.Description =
                "Vaccinations whose validity ends within the given number of days, soonest first. " +
                "Expired ones are included only when requested; vaccinations without a validity date never are.";
        });
    }

    public override async Task HandleAsync(ListExpiringVaccinationsRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new ListExpiringVaccinationsQuery(req.ExpiresInDays, req.IncludeExpired, req.InShelterOnly), ct);
        await this.SendResultAsync(result, ct);
    }
}
