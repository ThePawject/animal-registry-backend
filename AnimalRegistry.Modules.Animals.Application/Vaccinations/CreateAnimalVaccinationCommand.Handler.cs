using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed class CreateAnimalVaccinationCommandHandler(
    IAnimalRepository animalRepository,
    IAnimalVaccinationRepository vaccinationRepository,
    ReplacedVaccinationResolver replacedVaccinationResolver,
    IAnimalsUnitOfWork unitOfWork,
    IBlobStorageService blobStorageService,
    ICurrentUser currentUser)
    : IRequestHandler<CreateAnimalVaccinationCommand, Result<AnimalVaccinationDto>>
{
    public async Task<Result<AnimalVaccinationDto>> Handle(CreateAnimalVaccinationCommand request,
        CancellationToken cancellationToken)
    {
        if (!await animalRepository.ExistsAsync(request.AnimalId, currentUser.ShelterId, cancellationToken))
        {
            return Result<AnimalVaccinationDto>.NotFound("Animal not found.");
        }

        var replacedResult = await replacedVaccinationResolver.ResolveAsync(request.ReplacesVaccinationId,
            request.AnimalId, null, currentUser.ShelterId, cancellationToken);
        if (replacedResult.IsFailure)
        {
            return Result<AnimalVaccinationDto>.ValidationError(replacedResult.Error!);
        }

        var replaced = replacedResult.Value!;
        var now = DateTimeOffset.UtcNow;
        var vaccination = AnimalVaccination.Create(request.AnimalId, currentUser.ShelterId, request.Data,
            replaced.Vaccination, replaced.IsReplacedElsewhere, currentUser.Email, now);
        vaccinationRepository.Add(vaccination);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return Result<AnimalVaccinationDto>.ValidationError(saveResult.Error!);
        }

        return Result<AnimalVaccinationDto>.Success(
            AnimalVaccinationDto.FromDomain(vaccination, null, now, blobStorageService));
    }
}
