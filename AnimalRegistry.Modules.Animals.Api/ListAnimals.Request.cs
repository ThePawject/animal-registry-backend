using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared.Pagination;

namespace AnimalRegistry.Modules.Animals.Api;

public sealed record ListAnimalsRequest : PaginationRequest
{
    public const string Route = "/animals";

    public string? KeyWordSearch { get; init; }
    public List<AnimalSpecies>? Species { get; init; }
    public bool? IsInShelter { get; init; }
}