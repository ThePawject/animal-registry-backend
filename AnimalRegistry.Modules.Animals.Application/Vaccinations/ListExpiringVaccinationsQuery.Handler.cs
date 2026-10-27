using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.Pagination;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed class ListExpiringVaccinationsQueryHandler(
    IVaccinationQuery vaccinationQuery,
    IBlobStorageService blobStorageService,
    ICurrentUser currentUser)
    : IRequestHandler<ListExpiringVaccinationsQuery, Result<PagedResult<VaccinationWithAnimalDto>>>
{
    public async Task<Result<PagedResult<VaccinationWithAnimalDto>>> Handle(ListExpiringVaccinationsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var (validFrom, validBefore) = request.Validity switch
        {
            VaccinationValidityFilter.All => ((DateTimeOffset?)null, (DateTimeOffset?)null),
            VaccinationValidityFilter.Expired => ExpiringVaccinationsWindow.ResolveExpired(now),
            _ => ExpiringVaccinationsWindow.Resolve(request.ExpiresInDays, request.IncludeExpired, now),
        };

        var page = await vaccinationQuery.ListAsync(currentUser.ShelterId, validFrom, validBefore,
            request.InShelterOnly, request.Page, request.PageSize, cancellationToken);

        var items = page.Items
            .Select(v => VaccinationWithAnimalDto.FromDomain(v, now, blobStorageService))
            .ToList();
        return Result<PagedResult<VaccinationWithAnimalDto>>.Success(
            new PagedResult<VaccinationWithAnimalDto>(items, page.TotalCount, page.Page, page.PageSize));
    }
}
