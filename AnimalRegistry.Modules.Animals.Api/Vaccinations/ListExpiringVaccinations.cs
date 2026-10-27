using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.Pagination;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

internal sealed class ListExpiringVaccinations(IMediator mediator)
    : Endpoint<ListExpiringVaccinationsRequest, PagedResult<VaccinationWithAnimalDto>>
{
    public override void Configure()
    {
        Get(ListExpiringVaccinationsRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Options(b => b.RequireShelterFeature(AnimalVaccinationFeature.FeatureKey));
        Summary(s =>
        {
            s.Summary = "List vaccinations";
            s.Description =
                "By default, vaccinations whose validity ends within the given number of days, soonest first. " +
                "Expired ones are included only when requested; vaccinations without a validity date never are. " +
                "With validity=Expired only the expired ones are listed, with validity=All every vaccination " +
                "that has not been replaced, those without a validity date last. The result is paged.";
        });
    }

    public override async Task HandleAsync(ListExpiringVaccinationsRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new ListExpiringVaccinationsQuery(req.Validity, req.ExpiresInDays, req.IncludeExpired, req.InShelterOnly,
                req.Page, req.PageSize), ct);
        await this.SendResultAsync(result, ct);
    }
}
