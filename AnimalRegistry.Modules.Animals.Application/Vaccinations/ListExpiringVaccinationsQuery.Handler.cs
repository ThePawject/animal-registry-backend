using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed class ListExpiringVaccinationsQueryHandler(
    IVaccinationQuery vaccinationQuery,
    IBlobStorageService blobStorageService,
    ICurrentUser currentUser)
    : IRequestHandler<ListExpiringVaccinationsQuery, Result<IReadOnlyList<VaccinationWithAnimalDto>>>
{
    public async Task<Result<IReadOnlyList<VaccinationWithAnimalDto>>> Handle(ListExpiringVaccinationsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var (validFrom, validBefore) =
            ExpiringVaccinationsWindow.Resolve(request.ExpiresInDays, request.IncludeExpired, now);

        var vaccinations = await vaccinationQuery.GetExpiringAsync(currentUser.ShelterId, validFrom,
            validBefore, request.InShelterOnly, cancellationToken);

        IReadOnlyList<VaccinationWithAnimalDto> dtos = vaccinations
            .Select(v => VaccinationWithAnimalDto.FromDomain(v, now, blobStorageService))
            .ToList();
        return Result<IReadOnlyList<VaccinationWithAnimalDto>>.Success(dtos);
    }
}
