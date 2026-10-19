using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed class UpdateAnimalVaccinationCommandHandler(
    IAnimalVaccinationRepository vaccinationRepository,
    ReplacedVaccinationResolver replacedVaccinationResolver,
    IAnimalsUnitOfWork unitOfWork,
    IBlobStorageService blobStorageService,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateAnimalVaccinationCommand, Result<AnimalVaccinationDto>>
{
    public async Task<Result<AnimalVaccinationDto>> Handle(UpdateAnimalVaccinationCommand request,
        CancellationToken cancellationToken)
    {
        var vaccination = await vaccinationRepository.GetByIdAsync(request.VaccinationId, request.AnimalId,
            currentUser.ShelterId, cancellationToken);
        if (vaccination is null)
        {
            return Result<AnimalVaccinationDto>.NotFound("Vaccination not found.");
        }

        var replacedResult = await replacedVaccinationResolver.ResolveAsync(request.ReplacesVaccinationId,
            request.AnimalId, vaccination.Id, currentUser.ShelterId, cancellationToken);
        if (replacedResult.IsFailure)
        {
            return Result<AnimalVaccinationDto>.ValidationError(replacedResult.Error!);
        }

        var replaced = replacedResult.Value!;
        var now = DateTimeOffset.UtcNow;
        vaccination.Update(request.Data, replaced.Vaccination, replaced.IsReplacedElsewhere, now);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return Result<AnimalVaccinationDto>.ValidationError(saveResult.Error!);
        }

        var replacing = await vaccinationRepository.FindReplacingAsync(vaccination.Id, currentUser.ShelterId,
            cancellationToken);
        return Result<AnimalVaccinationDto>.Success(
            AnimalVaccinationDto.FromDomain(vaccination, replacing?.Id, now, blobStorageService));
    }
}
