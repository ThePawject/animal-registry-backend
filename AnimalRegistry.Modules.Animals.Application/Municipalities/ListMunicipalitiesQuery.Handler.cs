using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Municipalities;

internal sealed class ListMunicipalitiesQueryHandler(
    IMunicipalityRepository municipalityRepository,
    ICurrentUser currentUser)
    : IRequestHandler<ListMunicipalitiesQuery, Result<IReadOnlyList<MunicipalityDto>>>
{
    public async Task<Result<IReadOnlyList<MunicipalityDto>>> Handle(ListMunicipalitiesQuery request,
        CancellationToken cancellationToken)
    {
        var municipalities = await municipalityRepository.ListAsync(
            currentUser.ShelterId,
            request.IncludeArchived,
            cancellationToken);

        IReadOnlyList<MunicipalityDto> dtos = municipalities.Select(MunicipalityDto.FromDomain).ToList();
        return Result<IReadOnlyList<MunicipalityDto>>.Success(dtos);
    }
}
