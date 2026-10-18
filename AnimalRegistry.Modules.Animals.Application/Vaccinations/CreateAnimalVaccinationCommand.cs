using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed record CreateAnimalVaccinationCommand(Guid AnimalId, AnimalVaccinationData Data,
    Guid? ReplacesVaccinationId)
    : IRequest<Result<AnimalVaccinationDto>>;
