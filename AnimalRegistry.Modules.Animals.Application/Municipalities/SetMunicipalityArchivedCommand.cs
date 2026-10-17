using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Municipalities;

internal sealed record SetMunicipalityArchivedCommand(Guid Id, bool IsArchived) : IRequest<Result<MunicipalityDto>>;
