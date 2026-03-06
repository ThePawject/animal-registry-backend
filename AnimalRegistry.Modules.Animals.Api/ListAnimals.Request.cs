using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared.Pagination;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

public sealed record ListAnimalsRequest : PaginationRequest
{
    public const string Route = "/animals";

    public string? KeyWordSearch { get; init; }
    
    [QueryParam]
    public List<AnimalSpecies>? Species { get; init; }
    
    [QueryParam]
    public bool? IsInShelter { get; init; }
}