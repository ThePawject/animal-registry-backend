using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Municipalities;

internal sealed class SetMunicipalityArchivedCommandHandler(
    IMunicipalityRepository municipalityRepository,
    ICurrentUser currentUser)
    : IRequestHandler<SetMunicipalityArchivedCommand, Result<MunicipalityDto>>
{
    public async Task<Result<MunicipalityDto>> Handle(SetMunicipalityArchivedCommand request,
        CancellationToken cancellationToken)
    {
        var municipality =
            await municipalityRepository.GetByIdAsync(request.Id, currentUser.ShelterId, cancellationToken);
        if (municipality is null)
        {
            return Result<MunicipalityDto>.NotFound($"Municipality with id {request.Id} not found.");
        }

        if (request.IsArchived)
        {
            municipality.Archive();
        }
        else
        {
            municipality.Restore();
        }

        await municipalityRepository.UpdateAsync(municipality, cancellationToken);

        return Result<MunicipalityDto>.Success(MunicipalityDto.FromDomain(municipality));
    }
}
