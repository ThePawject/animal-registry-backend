using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Municipalities;

internal sealed class CreateMunicipalityCommandHandler(
    IMunicipalityRepository municipalityRepository,
    ICurrentUser currentUser)
    : IRequestHandler<CreateMunicipalityCommand, Result<MunicipalityDto>>
{
    public async Task<Result<MunicipalityDto>> Handle(CreateMunicipalityCommand request,
        CancellationToken cancellationToken)
    {
        if (!await municipalityRepository.IsNameUniqueAsync(request.Name, currentUser.ShelterId, null,
                cancellationToken))
        {
            return Result<MunicipalityDto>.ValidationError($"Municipality '{request.Name.Trim()}' already exists.");
        }

        var municipality = Municipality.Create(currentUser.ShelterId, request.Name);
        if (!await municipalityRepository.AddAsync(municipality, cancellationToken))
        {
            return Result<MunicipalityDto>.ValidationError($"Municipality '{municipality.Name}' already exists.");
        }

        return Result<MunicipalityDto>.Success(MunicipalityDto.FromDomain(municipality));
    }
}
