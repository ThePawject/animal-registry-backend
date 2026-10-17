using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Municipalities;

internal sealed class UpdateMunicipalityCommandHandler(
    IMunicipalityRepository municipalityRepository,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateMunicipalityCommand, Result<MunicipalityDto>>
{
    public async Task<Result<MunicipalityDto>> Handle(UpdateMunicipalityCommand request,
        CancellationToken cancellationToken)
    {
        var municipality =
            await municipalityRepository.GetByIdAsync(request.Id, currentUser.ShelterId, cancellationToken);
        if (municipality is null)
        {
            return Result<MunicipalityDto>.NotFound($"Municipality with id {request.Id} not found.");
        }

        if (!await municipalityRepository.IsNameUniqueAsync(request.Name, currentUser.ShelterId, request.Id,
                cancellationToken))
        {
            return Result<MunicipalityDto>.ValidationError($"Municipality '{request.Name.Trim()}' already exists.");
        }

        municipality.Rename(request.Name);
        if (!await municipalityRepository.UpdateAsync(municipality, cancellationToken))
        {
            return Result<MunicipalityDto>.ValidationError($"Municipality '{municipality.Name}' already exists.");
        }

        return Result<MunicipalityDto>.Success(MunicipalityDto.FromDomain(municipality));
    }
}
