using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed class CreateAnimalVaccinationCommandHandler(
    IAnimalRepository animalRepository,
    IAnimalVaccinationRepository vaccinationRepository,
    IAnimalsUnitOfWork unitOfWork,
    IBlobStorageService blobStorageService,
    ICurrentUser currentUser)
    : IRequestHandler<CreateAnimalVaccinationCommand, Result<AnimalVaccinationDto>>
{
    public async Task<Result<AnimalVaccinationDto>> Handle(CreateAnimalVaccinationCommand request,
        CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(request.AnimalId, currentUser.ShelterId, cancellationToken);
        if (animal is null)
        {
            return Result<AnimalVaccinationDto>.NotFound("Animal not found.");
        }

        var now = DateTimeOffset.UtcNow;
        var vaccination = AnimalVaccination.Create(animal.Id, animal.ShelterId, request.Data, currentUser.Email, now);
        vaccinationRepository.Add(vaccination);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return Result<AnimalVaccinationDto>.ValidationError(saveResult.Error!);
        }

        return Result<AnimalVaccinationDto>.Success(
            AnimalVaccinationDto.FromDomain(vaccination, now, blobStorageService));
    }
}
