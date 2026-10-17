using AnimalRegistry.Modules.Animals.Application.AnimalHealth;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed record SetVaccinationAttachmentCommand(Guid AnimalId, Guid VaccinationId, DocumentUploadInfo File)
    : IRequest<Result<AnimalVaccinationDto>>;
