using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.Pagination;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed record ListAnimalsQuery(int Page, int PageSize, string? KeyWordSearch)
    : IRequest<Result<PagedResult<AnimalListItemDto>>>;