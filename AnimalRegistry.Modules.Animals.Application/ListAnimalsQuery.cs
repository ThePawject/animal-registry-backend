using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.Pagination;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed record ListAnimalsQuery(
    int Page,
    int PageSize,
    string? KeyWordSearch,
    List<AnimalSpecies>? Species,
    bool? IsInShelter)
    : IRequest<Result<PagedResult<AnimalListItemDto>>>;