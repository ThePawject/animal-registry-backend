using AnimalRegistry.Shared.Pagination;

namespace AnimalRegistry.Modules.Animals.Api;

public sealed record ListAnimalsRequest : PaginationRequest
{
    public const string Route = "/animals";
}