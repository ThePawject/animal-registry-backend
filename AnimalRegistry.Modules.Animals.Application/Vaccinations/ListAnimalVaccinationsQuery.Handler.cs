using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed class ListAnimalVaccinationsQueryHandler(
    IAnimalRepository animalRepository,
    IAnimalVaccinationRepository vaccinationRepository,
    IBlobStorageService blobStorageService,
    ICurrentUser currentUser)
    : IRequestHandler<ListAnimalVaccinationsQuery, Result<IReadOnlyList<AnimalVaccinationDto>>>
{
    public async Task<Result<IReadOnlyList<AnimalVaccinationDto>>> Handle(ListAnimalVaccinationsQuery request,
        CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(request.AnimalId, currentUser.ShelterId, cancellationToken);
        if (animal is null)
        {
            return Result<IReadOnlyList<AnimalVaccinationDto>>.NotFound("Animal not found.");
        }

        var vaccinations = await vaccinationRepository.ListByAnimalIdAsync(animal.Id, currentUser.ShelterId,
            cancellationToken);
        var now = DateTimeOffset.UtcNow;

        IReadOnlyList<AnimalVaccinationDto> dtos = vaccinations
            .Select(v => AnimalVaccinationDto.FromDomain(v, now, blobStorageService))
            .ToList();
        return Result<IReadOnlyList<AnimalVaccinationDto>>.Success(dtos);
    }
}
