using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Municipalities;

internal sealed record ListMunicipalitiesQuery(bool IncludeArchived)
    : IRequest<Result<IReadOnlyList<MunicipalityDto>>>;
