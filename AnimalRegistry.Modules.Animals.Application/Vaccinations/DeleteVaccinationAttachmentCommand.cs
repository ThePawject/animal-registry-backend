using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed record DeleteVaccinationAttachmentCommand(Guid AnimalId, Guid VaccinationId) : IRequest<Result>;
