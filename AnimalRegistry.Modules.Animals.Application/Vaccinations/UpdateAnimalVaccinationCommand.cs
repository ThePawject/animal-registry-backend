using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed record UpdateAnimalVaccinationCommand(Guid AnimalId, Guid VaccinationId, AnimalVaccinationData Data)
    : IRequest<Result<AnimalVaccinationDto>>;
