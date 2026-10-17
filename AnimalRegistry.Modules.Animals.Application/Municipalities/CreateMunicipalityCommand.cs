using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Municipalities;

internal sealed record CreateMunicipalityCommand(string Name) : IRequest<Result<MunicipalityDto>>;
